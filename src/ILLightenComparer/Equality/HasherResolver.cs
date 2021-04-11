using System;
using System.Collections.Generic;
using System.Linq;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Equality.Hashers;
using ILLightenComparer.Extensions;
using ILLightenComparer.Shared;
using ILLightenComparer.Variables;

namespace ILLightenComparer.Equality
{
    internal sealed class HasherResolver
    {
        private readonly IConfigurationProvider _configuration;
        private readonly IReadOnlyCollection<Func<IVariable, IHasherEmitter>> _hashersFactories;

        public HasherResolver(EqualityContext context, MembersProvider membersProvider, IConfigurationProvider configuration)
        {
            _configuration = configuration;

            _hashersFactories = new Func<IVariable, IHasherEmitter>[] {
                variable => NullableHasher.Create(this, variable),
                variable => StringHasher.Create(_configuration, variable),
                BasicHasher.Create,
                variable => MembersHasher.Create(this, membersProvider, _configuration, variable),
                variable => ArrayHasher.Create(this, _configuration, variable),
                variable => EnumerablesHasher.Create(this, _configuration, variable),
                variable => IndirectHasher.Create(context, variable)
            };
        }

        public IHasherEmitter GetHasherEmitter(IVariable variable)
        {
            var hasCustomComparer = _configuration.HasCustomEqualityComparer(variable.VariableType);
            if (hasCustomComparer) {
                return IndirectHasher.Create(variable);
            }

            var hasher = _hashersFactories
                         .Select(factory => factory(variable))
                         .FirstOrDefault(x => x != null);

            if (hasher == null) {
                throw new NotSupportedException($"{variable.VariableType.DisplayName()} is not supported.");
            }

            return hasher;
        }
    }
}
