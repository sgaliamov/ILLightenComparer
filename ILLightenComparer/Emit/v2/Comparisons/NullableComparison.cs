﻿using System;
using System.Reflection;
using ILLightenComparer.Emit.Extensions;
using ILLightenComparer.Emit.Shared;
using ILLightenComparer.Emit.v2.Variables;

namespace ILLightenComparer.Emit.v2.Comparisons
{
    internal sealed class NullableComparison : ICompareEmitterAcceptor
    {
        private NullableComparison(IVariable variable)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }

        public IVariable Variable { get; }

        public ILEmitter Accept(CompareEmitter visitor, ILEmitter il)
        {
            return visitor.Visit(this, il);
        }

        public static ICompareEmitterAcceptor Create(MemberInfo memberInfo)
        {
            var variable = VariableFactory.Create(memberInfo);

            return Create(variable);
        }

        public static ICompareEmitterAcceptor Create(IVariable variable)
        {
            if (variable.VariableType.IsNullable())
            {
                return new NullableComparison(variable);
            }

            return null;
        }
    }
}
