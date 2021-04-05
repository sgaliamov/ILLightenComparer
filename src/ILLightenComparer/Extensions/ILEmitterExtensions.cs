﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Variables;
using Illuminator;
using Illuminator.Extensions;
using static Illuminator.FunctionalExtensions;

namespace ILLightenComparer.Extensions
{
    internal static class ILEmitterExtensions
    {
        private const byte ShortFormLimit = byte.MaxValue; // 255
        private const string LengthMethodName = nameof(Array.Length);
        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));
        private static readonly MethodInfo GetComparerMethod = typeof(IComparerProvider).GetMethod(nameof(IComparerProvider.GetComparer));
        private static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), Type.EmptyTypes);

        public static ILEmitter EmitArrayLength(this ILEmitter il, Type arrayType, LocalBuilder array, out LocalBuilder count) =>
            il.Ldloc(array)
              .Call(arrayType.GetPropertyGetter(LengthMethodName))
              .Stloc(typeof(int), out count);

        public static ILEmitter EmitArraySorting(this ILEmitter il, bool hasCustomComparer, Type elementType, params LocalBuilder[] arrays)
        {
            var useSimpleSorting = !hasCustomComparer && elementType.GetUnderlyingType().ImplementsGenericInterface(typeof(IComparable<>));

            if (useSimpleSorting) {
                foreach (var array in arrays) {
                    // todo: 2. compare default sorting and sorting with generated comparer - TrySZSort can work faster
                    EmitSortArray(il, elementType, array);
                }
            } else {
                var getComparerMethod = GetComparerMethod.MakeGenericMethod(elementType);

                il.LoadArgument(Arg.Context)
                  .CallMethod(getComparerMethod)
                  .Stloc(getComparerMethod.ReturnType, out var comparer);

                foreach (var array in arrays) {
                    EmitSortArray(il, elementType, array, comparer);
                }
            }

            return il;
        }

        public static ILEmitter EmitDispose(this ILEmitter il, LocalBuilder local) => il
                                                                                      .LoadCaller(local)
                                                                                      .ExecuteIf(local.LocalType.IsValueType, Constrained(local.LocalType))
                                                                                      .Call(DisposeMethod);

        private static void EmitSortArray(ILEmitter il, Type elementType, LocalBuilder array, LocalBuilder comparer)
        {
            var copyMethod = ToArrayMethod.MakeGenericMethod(elementType);
            var sortMethod = GetArraySortWithComparer(elementType);

            il.Ldloc(array)
              .CallMethod(copyMethod)
              .Stloc(array)
              .Ldloc(array)
              .Ldloc(comparer)
              .Call(sortMethod);
        }

        private static void EmitSortArray(ILEmitter il, Type elementType, LocalBuilder array)
        {
            var copyMethod = ToArrayMethod.MakeGenericMethod(elementType);
            var sortMethod = GetArraySortMethod(elementType);

            il.Ldloc(array)
              .CallMethod(copyMethod)
              .Stloc(array)
              .Ldloc(array)
              .CallMethod(sortMethod);
        }

        private static MethodInfo GetArraySortWithComparer(Type elementType) =>
            typeof(Array)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == nameof(Array.Sort) && x.IsGenericMethodDefinition)
                .Single(x => {
                    var parameters = x.GetParameters();

                    return parameters.Length == 2
                           && parameters[0].ParameterType.IsArray
                           && parameters[1].ParameterType.IsGenericType
                           && parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(IComparer<>);
                })
                .MakeGenericMethod(elementType);

        private static MethodInfo GetArraySortMethod(Type elementType) =>
            typeof(Array)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == nameof(Array.Sort) && x.IsGenericMethodDefinition)
                .Single(x => {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType.IsArray;
                })
                .MakeGenericMethod(elementType);

        public static ILEmitter Ceq(this ILEmitter il, ILEmitterFunc a, ILEmitterFunc b, out LocalBuilder local) =>
            il.Ceq(a, b)
              .Stloc(typeof(int), out local);

        public static ILEmitter Stloc(this ILEmitter il, Type type, out LocalBuilder local) =>
            il.DeclareLocal(type, out local)
              .Stloc(local);

        public static ILEmitter If(this ILEmitter il, ILEmitterFunc action, ILEmitterFunc whenTrue, ILEmitterFunc elseAction) =>
            action(il)
                .Brfalse(out var elseBlock)
                .Emit(whenTrue)
                .Br(out var next)
                .MarkLabel(elseBlock)
                .Emit(elseAction)
                .MarkLabel(next);

        public static ILEmitter If(this ILEmitter il, ILEmitterFunc action, ILEmitterFunc whenTrue) =>
            action(il)
                .Brfalse(out var exit)
                .Emit(whenTrue)
                .MarkLabel(exit);

        public static ILEmitter ExecuteIf(this ILEmitter il, bool condition, params ILEmitterFunc[] actions) => 
            condition ? il.Emit(actions) : il;

        // todo: 3. make Constrained when method is virtual and caller is value type
        public static ILEmitter LoadCaller(this ILEmitter il, LocalVariableInfo local) =>
            local.LocalType.IsValueType ? il.Ldloca(local) : il.Ldloc(local);

        public static ILEmitter Ret(this ILEmitter il, int value) => il.Ldc_I4(value).Ret();

        public static ILEmitter Ret(this ILEmitter il, LocalBuilder local) => il.Ldloc(local).Ret();

        public static ILEmitter LoadArgument(this ILEmitter il, ushort argumentIndex)
        {
            switch (argumentIndex) {
                case 0: return Emit(OpCodes.Ldarg_0);
                case 1: return Emit(OpCodes.Ldarg_1);
                case 2: return Emit(OpCodes.Ldarg_2);
                case 3: return Emit(OpCodes.Ldarg_3);
                default:
                    var opCode = argumentIndex <= ShortFormLimit ? OpCodes.Ldarg_S : OpCodes.Ldarg;
                    return Emit(opCode, argumentIndex);
            }
        }

        public static ILEmitter LoadArgumentAddress(this ILEmitter il, ushort argumentIndex)
        {
            var opCode = argumentIndex <= ShortFormLimit ? OpCodes.Ldarga_S : OpCodes.Ldarga;
            return Emit(opCode, argumentIndex);
        }

        public static ILEmitter CallMethod(this ILEmitter il, MethodInfo methodInfo, params ILEmitterFunc[] parameters)
        {
            if (!(methodInfo is MethodBuilder)) {
                var methodParametesLenght = methodInfo.GetParameters().Length;

                if ((methodInfo.IsStatic && methodParametesLenght != parameters.Length)
                    || (!methodInfo.IsStatic && methodParametesLenght != parameters.Length - 1)) {
                    throw new ArgumentException($"Amount of parameters does not match method {methodInfo} signature.");
                }
            }

            foreach (var parameter in parameters) {
                parameter(il);
            }

            return il.Call(methodInfo, parameters);
        }

        public static ILEmitter CallMethod(this ILEmitter il, MethodInfo methodInfo)
        {
            var owner = methodInfo.DeclaringType;
            if (owner == typeof(ValueType)) {
                owner = methodInfo.ReflectedType; // todo: 0. test
            }

            if (owner == null) {
                throw new InvalidOperationException(
                    $"It's not expected that {methodInfo.DisplayName()} doesn't have a declaring type.");
            }

            if (methodInfo.IsGenericMethodDefinition) {
                throw new InvalidOperationException(
                    $"Generic method {methodInfo.DisplayName()} is not initialized.");
            }

            // if the method belongs to Enum type, them it should be called as virtual and with constrained prefix
            // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.constrained
            //var isEnum = owner.IsAssignableFrom(typeof(Enum));
            var opCode = methodInfo.IsStatic || owner.IsValueType || owner.IsSealed || !methodInfo.IsVirtual // todo: 0. test
                ? OpCodes.Call
                : OpCodes.Callvirt;

            //if (isEnum) {
            //    Constrained(owner); // todo: 0. test
            //}

            return Emit(opCode, methodInfo);
        }
    }
}