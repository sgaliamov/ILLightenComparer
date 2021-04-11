using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Comparer.Comparisons
{
    internal sealed class IntegralsComparison : IComparisonEmitter
    {
        public static IntegralsComparison Create(IVariable variable)
        {
            if (variable.VariableType.GetUnderlyingType().IsIntegral()) {
                return new IntegralsComparison(variable);
            }

            return null;
        }

        private readonly IVariable _variable;

        private IntegralsComparison(IVariable variable)
        {
            _variable = variable;
        }

        public ILEmitter Emit(ILEmitter il, Label _) => il.Sub(_variable.Load(Arg.X), _variable.Load(Arg.Y));

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => il.EmitReturnIfTruthy(next);
    }
}
