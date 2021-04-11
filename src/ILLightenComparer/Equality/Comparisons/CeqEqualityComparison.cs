using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Equality.Comparisons
{
    internal sealed class CeqEqualityComparison : IComparisonEmitter
    {
        public static CeqEqualityComparison Create(IVariable variable)
        {
            if (variable.VariableType.GetUnderlyingType().IsCeqCompatible()) {
                return new CeqEqualityComparison(variable);
            }

            return null;
        }

        private readonly IVariable _variable;

        private CeqEqualityComparison(IVariable variable)
        {
            _variable = variable;
        }

        public ILEmitter Emit(ILEmitter il, Label _) => il.Ceq(_variable.Load(Arg.X), _variable.Load(Arg.Y));

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => il.EmitReturnIfFalsy(next);
    }
}
