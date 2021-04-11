using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Equality.Comparisons
{
    internal sealed class OperatorEqualityComparison : IComparisonEmitter
    {
        public static OperatorEqualityComparison Create(IVariable variable)
        {
            var variableType = variable.VariableType.GetUnderlyingType();
            var equalityMethod = variableType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);

            if (equalityMethod != null) {
                return new OperatorEqualityComparison(variable, equalityMethod);
            }

            return null;
        }

        private readonly MethodInfo _equalityMethod;
        private readonly IVariable _variable;

        private OperatorEqualityComparison(IVariable variable, MethodInfo equalityMethod)
        {
            _variable = variable;
            _equalityMethod = equalityMethod;
        }

        public ILEmitter Emit(ILEmitter il, Label _) =>
            il.CallMethod(
                _equalityMethod,
                _variable.Load(Arg.X),
                _variable.Load(Arg.Y));

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => il.EmitReturnIfFalsy(next);
    }
}
