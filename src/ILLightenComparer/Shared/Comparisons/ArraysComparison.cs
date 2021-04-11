using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Shared.Comparisons
{
    internal sealed class ArraysComparison : IComparisonEmitter
    {
        public static ArraysComparison Create(ArrayComparisonEmitter arrayComparisonEmitter, IConfigurationProvider configuration, IVariable variable)
        {
            var variableType = variable.VariableType;
            if (variableType.IsArray && variableType.GetArrayRank() == 1) {
                return new ArraysComparison(arrayComparisonEmitter, configuration, variable);
            }

            return null;
        }

        private readonly ArrayComparisonEmitter _arrayComparisonEmitter;
        private readonly IConfigurationProvider _configuration;
        private readonly IVariable _variable;

        private ArraysComparison(ArrayComparisonEmitter arrayComparisonEmitter, IConfigurationProvider configuration, IVariable variable)
        {
            _configuration = configuration;
            _variable = variable;
            _arrayComparisonEmitter = arrayComparisonEmitter;
        }

        public ILEmitter Emit(ILEmitter il, Label gotoNext)
        {
            var arrayType = _variable.VariableType;
            var (arrayX, arrayY) = _arrayComparisonEmitter.EmitLoad(_variable, il, gotoNext);

            if (_configuration.Get(_variable.OwnerType).IgnoreCollectionOrder) {
                var elementType = arrayType.GetElementType();
                var hasCustomComparer = _configuration.HasCustomComparer(elementType);
                il.EmitArraySorting(hasCustomComparer, elementType, arrayX, arrayY);
            }

            return _arrayComparisonEmitter.EmitCompareArrays(il, arrayType, _variable.OwnerType, arrayX, arrayY, gotoNext);
        }

        public ILEmitter EmitCheckForResult(ILEmitter il, Label _) => il;
    }
}
