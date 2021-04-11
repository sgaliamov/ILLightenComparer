using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Equality.Hashers
{
    internal sealed class BasicHasher : IHasherEmitter
    {
        public static BasicHasher Create(IVariable variable)
        {
            if (variable.VariableType.GetUnderlyingType().IsBasic()) {
                return new BasicHasher(variable);
            }

            return null;
        }

        private readonly MethodInfo _getHashMethod;
        private readonly IVariable _variable;

        private BasicHasher(IVariable variable)
        {
            _variable = variable;
            _getHashMethod = _variable.VariableType.GetUnderlyingType().GetMethod(nameof(GetHashCode));
        }

        public ILEmitter Emit(ILEmitter il) =>
            il.CallMethod(
                _getHashMethod,
                _variable.VariableType.IsValueType
                    ? _variable.LoadAddress(Arg.Input)
                    : _variable.Load(Arg.Input));

        public ILEmitter Emit(ILEmitter il, LocalBuilder _) => Emit(il);
    }
}
