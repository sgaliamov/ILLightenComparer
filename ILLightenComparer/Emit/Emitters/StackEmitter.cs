﻿using System.Reflection.Emit;
using ILLightenComparer.Emit.Extensions;
using ILLightenComparer.Emit.Members.Comparable;
using ILLightenComparer.Emit.Members.Integral;

namespace ILLightenComparer.Emit.Emitters
{
    internal sealed class StackEmitter
    {
        private readonly TypeBuilderContext _context;

        public StackEmitter(TypeBuilderContext context) => _context = context;

        public ILEmitter Visit(ComparableFieldMember member, ILEmitter il) =>
            il.LoadFieldAddress(member, 1)
              .LoadField(member, 2);

        public ILEmitter Visit(ComparablePropertyMember member, ILEmitter il) =>
            il.LoadProperty(member, 1)
              .DeclareLocal(member.MemberType.GetUnderlyingType(), out var local)
              .Store(local)
              .LoadAddress(local)
              .LoadProperty(member, 2);

        public ILEmitter Visit(StringFiledMember member, ILEmitter il)
        {
            var comparisonType = (int)_context.Configuration.StringComparisonType;

            il.LoadField(member, 1)
              .LoadField(member, 2)
              .Emit(OpCodes.Ldc_I4_S, comparisonType); // todo: use short form for constants

            return il;
        }

        public ILEmitter Visit(StringPropertyMember member, ILEmitter il)
        {
            var comparisonType = (int)_context.Configuration.StringComparisonType;

            il.LoadProperty(member, 1)
              .LoadProperty(member, 2)
              .Emit(OpCodes.Ldc_I4_S, comparisonType); // todo: use short form for constants

            return il;
        }

        public ILEmitter Visit(IntegralFiledMember member, ILEmitter il) =>
            il.LoadField(member, 1)
              .LoadField(member, 2);

        public ILEmitter Visit(IntegralPropertyMember member, ILEmitter il) =>
            il.LoadProperty(member, 1)
              .LoadProperty(member, 2);

        public ILEmitter Visit(NullablePropertyMember member, ILEmitter il) =>
            il.LoadProperty(member, 1)
              .LoadProperty(member, 2);
    }
}
