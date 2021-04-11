using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Shared.Comparisons
{
    // todo: 3. use as optimization. cannot be used now, because object of any type can be passed. DelayedCompare resolves it.
    internal sealed class ObjectComparison : IComparisonEmitter
    {
        public static ObjectComparison Create(IVariable variable) => variable.VariableType == typeof(object)
            ? new ObjectComparison(variable)
            : null;

        private readonly IVariable _variable;

        public ObjectComparison(IVariable variable)
        {
            _variable = variable;
        }

        public ILEmitter Emit(ILEmitter il, Label next) =>
            il.Emit(_variable.Load(Arg.Y))
              .Brtrue_S(_variable.Load(Arg.X), out var xIsNotNull)
              .Brfalse_S(next)
              .Ret(-1)
              .MarkLabel(xIsNotNull)
              .Brtrue_S(next)
              .Ret(1);

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => il;
    }
}
