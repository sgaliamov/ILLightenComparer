using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Equality.Comparisons
{
    internal sealed class BacisEqualityComparison : IComparisonEmitter
    {
        public static BacisEqualityComparison Create(IVariable variable)
        {
            var variableType = variable.VariableType.GetUnderlyingType();
            if (variableType.IsBasic()) {
                return new BacisEqualityComparison(variable);
            }

            return null;
        }

        private readonly MethodInfo _equalityMethod;
        private readonly IVariable _variable;

        private BacisEqualityComparison(IVariable variable)
        {
            _variable = variable;
            var variableType = _variable.VariableType;
            _equalityMethod = variableType.GetMethod(nameof(Equals), new[] { variableType });
        }

        public ILEmitter Emit(ILEmitter il, Label _) =>
            il.CallMethod(
                _equalityMethod,
                _variable.VariableType.IsValueType
                    ? _variable.LoadAddress(Arg.X)
                    : _variable.Load(Arg.X),
                _variable.Load(Arg.Y));

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => il.EmitReturnIfFalsy(next);
    }
}
