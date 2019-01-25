﻿using System.Reflection.Emit;
using ILLightenComparer.Emit.Extensions;
using ILLightenComparer.Emit.Shared;
using ILLightenComparer.Emit.v2.Variables;
using ILLightenComparer.Emit.v2.Visitors;

namespace ILLightenComparer.Emit.v2.Comparisons
{
    internal sealed class ComparablesComparison : IComparison
    {
        private ComparablesComparison(IVariable variable)
        {
            Variable = variable;
        }

        public IVariable Variable { get; }

        public ILEmitter Accept(CompareVisitor visitor, ILEmitter il, Label gotoNext)
        {
            return visitor.Visit(this, il, gotoNext);
        }

        public static ComparablesComparison Create(IVariable variable)
        {
            if (variable.VariableType.IsSealedComparable())
            {
                return new ComparablesComparison(variable);
            }

            return null;
        }
    }
}