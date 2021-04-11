using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static Illuminator.Functions;

namespace ILLightenComparer.Shared.Comparisons
{
    internal sealed class StringsComparison : IComparisonEmitter
    {
        public static StringsComparison Create(
            MethodInfo compareMethod,
            EmitterDelegate checkForIntermediateResultEmitter,
            IConfigurationProvider configuration,
            IVariable variable)
        {
            if (variable.VariableType == typeof(string)) {
                return new StringsComparison(compareMethod, checkForIntermediateResultEmitter, configuration, variable);
            }

            return null;
        }

        private readonly EmitterDelegate _checkForIntermediateResultEmitter;
        private readonly MethodInfo _compareMethod;
        private readonly int _stringComparisonType;
        private readonly IVariable _variable;

        private StringsComparison(
            MethodInfo compareMethod,
            EmitterDelegate checkForIntermediateResultEmitter,
            IConfigurationProvider configuration,
            IVariable variable)
        {
            _compareMethod = compareMethod;
            _checkForIntermediateResultEmitter = checkForIntermediateResultEmitter;
            _variable = variable;
            _stringComparisonType = (int)configuration.Get(_variable.OwnerType).StringComparisonType;
        }

        public ILEmitter Emit(ILEmitter il, Label _) =>
            il.CallMethod(
                _compareMethod,
                _variable.Load(Arg.X),
                _variable.Load(Arg.Y),
                Ldc_I4(_stringComparisonType));

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => _checkForIntermediateResultEmitter(il, next);
    }
}
