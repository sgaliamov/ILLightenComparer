using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Equality.Comparisons;
using ILLightenComparer.Extensions;
using ILLightenComparer.Shared;
using ILLightenComparer.Shared.Comparisons;
using ILLightenComparer.Variables;

namespace ILLightenComparer.Equality
{
    internal sealed class EqualityResolver : IResolver
    {
        private static readonly MethodInfo StringEqualsMethod = typeof(string).GetMethod(
            nameof(string.Equals),
            new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private static readonly MethodInfo DelayedEquals = typeof(IEqualityComparerContext)
            .GetMethod(nameof(IEqualityComparerContext.DelayedEquals));

        private readonly IReadOnlyCollection<Func<IVariable, IComparisonEmitter>> _comparisonFactories;
        private readonly IConfigurationProvider _configuration;

        public EqualityResolver(
            EqualityContext context,
            MembersProvider membersProvider,
            IConfigurationProvider configuration)
        {
            _configuration = configuration;

            var collectionComparer = new ArrayComparisonEmitter(this, CustomEmitters.EmitCheckIfLoopsAreDone, CustomEmitters.EmitReferenceComparison);

            _comparisonFactories = new Func<IVariable, IComparisonEmitter>[] {
                variable => NullableComparison.Create(this, CustomEmitters.EmitReturnIfFalsy, CustomEmitters.EmitCheckNullablesForValue, variable),
                CeqEqualityComparison.Create,
                variable => StringsComparison.Create(StringEqualsMethod, CustomEmitters.EmitReturnIfFalsy, _configuration, variable),
                OperatorEqualityComparison.Create,
                BacisEqualityComparison.Create,
                variable => MembersComparison.Create(this, membersProvider, variable),
                variable => ArraysComparison.Create(collectionComparer, _configuration, variable),
                variable => EnumerablesComparison.Create(this, collectionComparer, CustomEmitters.EmitCheckIfLoopsAreDone, _configuration, variable),
                variable => IndirectComparison.Create(
                    CustomEmitters.EmitReturnIfFalsy,
                    context.GetStaticEqualsMethodInfo,
                    DelayedEquals,
                    variable)
            };
        }

        public IComparisonEmitter GetComparisonEmitter(IVariable variable)
        {
            var hasCustomComparer = _configuration.HasCustomEqualityComparer(variable.VariableType);
            if (hasCustomComparer) {
                return IndirectComparison.Create(CustomEmitters.EmitReturnIfFalsy, DelayedEquals, variable);
            }

            var comparison = _comparisonFactories
                             .Select(factory => factory(variable))
                             .FirstOrDefault(x => x != null);

            if (comparison == null) {
                throw new NotSupportedException($"{variable.VariableType.DisplayName()} is not supported.");
            }

            return comparison;
        }
    }
}
