using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;

namespace ILLightenComparer.Shared.Comparisons
{
    internal sealed class MembersComparison : IComparisonEmitter
    {
        public static MembersComparison Create(
            IResolver resolver,
            MembersProvider membersProvider,
            IVariable variable)
        {
            if (variable.VariableType == typeof(object) || !variable.VariableType.IsHierarchical() || !(variable is ArgumentVariable)) {
                return null;
            }

            return new MembersComparison(resolver, membersProvider, variable);
        }

        private readonly MembersProvider _membersProvider;
        private readonly IResolver _resolver;
        private readonly IVariable _variable;

        private MembersComparison(
            IResolver resolver,
            MembersProvider membersProvider,
            IVariable variable)
        {
            _variable = variable;
            _resolver = resolver;
            _membersProvider = membersProvider;
        }

        public ILEmitter Emit(ILEmitter il, Label _)
        {
            var variableType = _variable.VariableType;
            Debug.Assert(!variableType.IsPrimitive(), $"{variableType.DisplayName()} is not expected.");

            var comparisons = _membersProvider
                              .GetMembers(variableType)
                              .Select(_resolver.GetComparisonEmitter)
                              .ToArray();

            for (var i = 0; i < comparisons.Length; i++) {
                using (il.LocalsScope()) {
                    il.DefineLabel(out var gotoNext)
                      .Emit(comparisons[i].Emit(gotoNext))
                      .Emit(comparisons[i].EmitCheckForResult(gotoNext))
                      .MarkLabel(gotoNext);
                }
            }

            return il;
        }

        public ILEmitter EmitCheckForResult(ILEmitter il, Label _) => il;
    }
}
