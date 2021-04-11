using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static ILLightenComparer.Extensions.Functions;

namespace ILLightenComparer.Equality.Hashers
{
    internal sealed class NullableHasher : IHasherEmitter
    {
        public static NullableHasher Create(HasherResolver resolver, IVariable variable)
        {
            if (variable.VariableType.IsNullable()) {
                return new NullableHasher(resolver, variable);
            }

            return null;
        }

        private readonly MethodInfo _hasValueMethod;
        private readonly HasherResolver _resolver;
        private readonly IVariable _variable;

        private NullableHasher(HasherResolver resolver, IVariable variable)
        {
            _resolver = resolver;
            _variable = variable;
            _hasValueMethod = _variable.VariableType.GetPropertyGetter("HasValue");
        }

        public ILEmitter Emit(ILEmitter il)
        {
            var variableType = _variable.VariableType;

            _variable
                .Load(il, Arg.Input)
                .Stloc(variableType, out var nullable)
                .CallMethod(_hasValueMethod, LoadLocalAddress(nullable))
                .Brtrue_S(out var next)
                .Ldc_I4(0)
                .Br(out var exit)
                .MarkLabel(next);

            var nullableVariable = new NullableVariables(
                variableType,
                _variable.OwnerType,
                new Dictionary<int, LocalBuilder>(1) { [Arg.Input] = nullable });

            return _resolver
                   .GetHasherEmitter(nullableVariable)
                   .Emit(il)
                   .MarkLabel(exit);
        }

        public ILEmitter Emit(ILEmitter il, LocalBuilder _) => Emit(il);
    }
}
