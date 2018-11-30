﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Emit.Emitters.Acceptors;
using ILLightenComparer.Emit.Extensions;
using ILLightenComparer.Emit.Reflection;

namespace ILLightenComparer.Emit.Emitters
{
    internal sealed class CompareEmitter
    {
        private readonly Context _context;
        private readonly StackEmitter _stackEmitter = new StackEmitter();

        public CompareEmitter(Context context) => _context = context;

        public ILEmitter Visit(IBasicAcceptor member, ILEmitter il)
        {
            var memberType = member.MemberType;
            var method = GetCompareToMethod(memberType);

            return member.Accept(_stackEmitter, il)
                         .Call(method)
                         .EmitReturnNotZero();
        }

        public ILEmitter Visit(IIntegralAcceptor member, ILEmitter il) =>
            member.Accept(_stackEmitter, il)
                  .Emit(OpCodes.Sub)
                  .EmitReturnNotZero();

        public ILEmitter Visit(INullableAcceptor member, ILEmitter il)
        {
            var memberType = member.MemberType;

            member.Accept(_stackEmitter, il)
                  .DefineLabel(out var next)
                  .Store(memberType, 0,out var n1)
                  .Store(memberType, 1, out var n2);

            CheckValuesForNull(il, member, n1, n2, next);

            if (memberType.GetUnderlyingType().IsSmallIntegral())
            {
                il.LoadAddress(n1)
                  .Call(member.GetValueMethod)
                  .LoadAddress(n2)
                  .Call(member.GetValueMethod)
                  .Emit(OpCodes.Sub);
            }
            else
            {
                var compareToMethod = GetCompareToMethod(memberType);

                il.LoadAddress(n1)
                  .Call(member.GetValueMethod)
                  .Store(memberType.GetUnderlyingType(), out var local)
                  .LoadAddress(local)
                  .LoadAddress(n2)
                  .Call(member.GetValueMethod)
                  .Call(compareToMethod);
            }

            // todo: nullable can be also complex struct, not only primitive types, so it can be considered as hierarchical

            il.EmitReturnNotZero(next);

            return il;
        }

        public ILEmitter Visit(IStringAcceptor member, ILEmitter il)
        {
            var comparisonType = (int)_context.GetConfiguration(member.DeclaringType).StringComparisonType;

            return member.Accept(_stackEmitter, il)
                         .Emit(OpCodes.Ldc_I4_S, comparisonType) // todo: use short form for constants
                         .Call(Method.StringCompare)
                         .EmitReturnNotZero();
        }

        public ILEmitter Visit(IHierarchicalAcceptor member, ILEmitter il)
        {
            // todo: IComparable can be null

            il.Emit(OpCodes.Ldarg_0); // todo: hash set will be hare
            member.Accept(_stackEmitter, il);

            var memberType = member.MemberType;
            if (memberType.IsValueType || memberType.IsSealed)
            {
                var comparerType = _context.GetComparerType(memberType);

                var method = comparerType.GetMethod(
                    MethodName.Compare,
                    new[]
                    {
                        typeof(HashSet<object>),
                        memberType,
                        memberType
                    });

                il.Call(method).EmitReturnNotZero();
            }
            else
            {
                throw new NotImplementedException();
            }

            return il;
        }

        private static MethodInfo GetCompareToMethod(Type memberType) =>
            memberType.GetCompareToMethod()
            ?? throw new ArgumentException(
                $"{memberType.DisplayName()} does not have {MethodName.CompareTo} method.");

        private static void CheckValuesForNull(
            ILEmitter il,
            INullableAcceptor member,
            LocalBuilder n1,
            LocalBuilder n2,
            Label next)
        {
            il.LoadAddress(n2)
              // var secondHasValue = n2->HasValue
              .Call(member.HasValueMethod)
              .Store(typeof(bool), out var secondHasValue)
              .LoadAddress(n1)
              // if n1->HasValue goto firstHasValue
              .Call(member.HasValueMethod)
              .Branch(OpCodes.Brtrue_S, out var firstHasValue)
              // if n2->HasValue goto returnZero
              .LoadLocal(secondHasValue)
              .Emit(OpCodes.Brfalse_S, next)
              // else return -1
              .Emit(OpCodes.Ldc_I4_M1)
              .Emit(OpCodes.Ret)
              // firstHasValue:
              .MarkLabel(firstHasValue)
              .LoadLocal(secondHasValue)
              .Branch(OpCodes.Brtrue_S, out var getValues)
              // return 1
              .Emit(OpCodes.Ldc_I4_1)
              .Emit(OpCodes.Ret)
              // getValues: load values
              .MarkLabel(getValues);
        }
    }
}
