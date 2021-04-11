using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static ILLightenComparer.Extensions.Functions;

namespace ILLightenComparer.Equality.Hashers
{
    /// <summary>
    ///     Delegates hashing to static method or delayed hasher in context.
    /// </summary>
    internal sealed class IndirectHasher : IHasherEmitter
    {
        private static readonly MethodInfo DelayedHash = typeof(IEqualityComparerContext)
            .GetMethod(nameof(IEqualityComparerContext.DelayedHash));

        public static IndirectHasher Create(IVariable variable) => new(DelayedHash.MakeGenericMethod(variable.VariableType), variable);

        public static IndirectHasher Create(EqualityContext context, IVariable variable)
        {
            var variableType = variable.VariableType;
            if (variableType != typeof(object) && variable is ArgumentVariable) {
                return null;
            }

            var staticHashMethod = context.GetStaticHashMethodInfo(variableType);
            var typeOfVariableCanBeChangedOnRuntime = !variableType.IsSealedType();
            var hashMethod = typeOfVariableCanBeChangedOnRuntime
                ? DelayedHash.MakeGenericMethod(variableType)
                : staticHashMethod;

            return new IndirectHasher(hashMethod, variable);
        }

        private readonly MethodInfo _hashMethod;
        private readonly IVariable _variable;

        private IndirectHasher(MethodInfo hashMethod, IVariable variable)
        {
            _hashMethod = hashMethod;
            _variable = variable;
        }

        public ILEmitter Emit(ILEmitter il) =>
            il.LoadArgument(Arg.Context)
              .CallMethod(
                  _hashMethod,
                  _variable.Load(Arg.Input),
                  LoadArgument(Arg.CycleSet));

        public ILEmitter Emit(ILEmitter il, LocalBuilder _) => Emit(il);
    }
}
