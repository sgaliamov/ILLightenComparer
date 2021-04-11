using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Variables;
using Illuminator;
using static Illuminator.Functions;

namespace ILLightenComparer.Extensions
{
    internal static class ILEmitterExtensions
    {
        private const byte ShortFormLimit = byte.MaxValue; // 255

        private const string LengthMethodName = nameof(Array.Length);
        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));
        private static readonly MethodInfo GetComparerMethod = typeof(IComparerProvider).GetMethod(nameof(IComparerProvider.GetComparer));
        private static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), Type.EmptyTypes);

        /// <summary>
        ///     Smart emission for Call methods.
        /// </summary>
        /// <param name="il">Self.</param>
        /// <param name="methodInfo">Static, virtual, instance method.</param>
        /// <param name="funcs">List of methods to prepare parameters in stack.</param>
        /// <returns>Self.</returns>
        public static ILEmitter CallMethod(
            this ILEmitter il,
            MethodInfo methodInfo,
            params ILEmitterFunc[] funcs)
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
            //if (isEnum) {
            //    Constrained(owner); // todo: 0. test
            //}

            return methodInfo.IsStatic || owner.IsValueType || owner.IsSealed || !methodInfo.IsVirtual // todo: 0. test
                ? il.Call(methodInfo, funcs)
                : il.Callvirt(methodInfo, funcs);
        }

        public static ILEmitter Cast<T>(this ILEmitter self, ILEmitterFunc value) => value(self).Cast(typeof(T));

        // todo: 3. test
        public static ILEmitter Cast(this ILEmitter self, Type type) => Type.GetTypeCode(type) switch {
            TypeCode.Int64 => self.Conv_I8(),
            TypeCode.Int32 => self.Conv_I4(),
            _ => type.IsValueType
                ? self.Unbox_Any(type)
                : self.Castclass(type)
        };

        public static ILEmitter Ceq(this ILEmitter il, ILEmitterFunc a, ILEmitterFunc b, out LocalBuilder local) =>
            il.Ceq(a, b)
              .Stloc(typeof(int), out local);

        public static ILEmitter EmitArrayLength(this ILEmitter il, Type arrayType, LocalBuilder array, out LocalBuilder count) =>
            il.CallMethod(arrayType.GetPropertyGetter(LengthMethodName), Ldloc(array))
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

                il.CallMethod(getComparerMethod, Functions.LoadArgument(Arg.Context))
                  .Stloc(getComparerMethod.ReturnType, out var comparer);

                foreach (var array in arrays) {
                    EmitSortArray(il, elementType, array, comparer);
                }
            }

            return il;
        }

        public static ILEmitter EmitDispose(this ILEmitter il, LocalBuilder local) =>
            il.LoadCaller(local)
              .EmitIf(local.LocalType.IsValueType, Constrained(local.LocalType))
              .CallMethod(DisposeMethod);

        public static ILEmitter EmitIf(this ILEmitter il, bool condition, params ILEmitterFunc[] actions) =>
            condition ? il.Emit(actions) : il;

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

        public static ILEmitter LoadLocalAddress(this ILEmitter self, LocalBuilder local) =>
            self.LoadLocalAddress(local.LocalIndex);

        public static ILEmitter LoadLocalAddress(this ILEmitter self, int localIndex) =>
            localIndex <= ShortFormLimit
                ? self.Ldloca_S((byte)localIndex)
                : self.Ldloca((short)localIndex);

        public static ILEmitter LoadArgument(this ILEmitter self, int argumentIndex) =>
            argumentIndex switch {
                0 => self.Ldarg_0(),
                1 => self.Ldarg_1(),
                2 => self.Ldarg_2(),
                3 => self.Ldarg_3(),
                _ => argumentIndex <= ShortFormLimit
                    ? self.Ldarg_S((byte)argumentIndex)
                    : self.Ldarg((short)argumentIndex)
            };

        public static ILEmitter LoadArgumentAddress(this ILEmitter self, ushort argumentIndex) =>
            argumentIndex <= ShortFormLimit
                ? self.Ldarga_S((byte)argumentIndex)
                : self.Ldarga((short)argumentIndex);

        // todo: 3. make Constrained when method is virtual and caller is value type
        public static ILEmitter LoadCaller(this ILEmitter il, LocalBuilder local) =>
            local.LocalType!.IsValueType
                ? il.LoadLocalAddress(local.LocalIndex)
                : il.Ldloc(local);

        public static ILEmitter Ret(this ILEmitter il, int value) => il.Ldc_I4(value).Ret();

        public static ILEmitter Ret(this ILEmitter il, LocalBuilder local) => il.Ldloc(local).Ret();

        public static ILEmitter Stloc(this ILEmitter il, Type type, out LocalBuilder local) =>
            il.DeclareLocal(type, out local)
              .Stloc(local);

        private static void EmitSortArray(ILEmitter il, Type elementType, LocalBuilder array, LocalBuilder comparer)
        {
            var copyMethod = ToArrayMethod.MakeGenericMethod(elementType);
            var sortMethod = GetArraySortWithComparer(elementType);

            il.CallMethod(copyMethod, Ldloc(array))
              .Stloc(array)
              .CallMethod(sortMethod, Ldloc(array), Ldloc(comparer));
        }

        private static void EmitSortArray(ILEmitter il, Type elementType, LocalBuilder array)
        {
            var copyMethod = ToArrayMethod.MakeGenericMethod(elementType);
            var sortMethod = GetArraySortMethod(elementType);

            il.CallMethod(copyMethod, Ldloc(array))
              .Stloc(array)
              .CallMethod(sortMethod, Ldloc(array));
        }

        private static MethodInfo GetArraySortMethod(Type elementType) =>
            typeof(Array)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == nameof(Array.Sort) && x.IsGenericMethodDefinition)
                .Single(x => {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType.IsArray;
                })
                .MakeGenericMethod(elementType);

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
    }
}
