﻿using System.Linq;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Shared;
using ILLightenComparer.Variables;
using Illuminator;
using static Illuminator.Functional;

namespace ILLightenComparer.Equality.Hashers
{
    internal sealed class MembersHasher : IHasherEmitter
    {
        private readonly IConfigurationProvider _configuration;
        private readonly MembersProvider _membersProvider;
        private readonly HasherResolver _resolver;
        private readonly IVariable _variable;

        private MembersHasher(
            HasherResolver resolver,
            MembersProvider membersProvider,
            IConfigurationProvider configuration,
            IVariable variable)
        {
            _variable = variable;
            _resolver = resolver;
            _membersProvider = membersProvider;
            _configuration = configuration;
        }

        public static MembersHasher Create(
            HasherResolver resolver,
            MembersProvider membersProvider,
            IConfigurationProvider configuration,
            IVariable variable)
        {
            if (variable.VariableType.IsHierarchical() && variable is ArgumentVariable) {
                return new MembersHasher(resolver, membersProvider, configuration, variable);
            }

            return null;
        }

        public ILEmitter Emit(ILEmitter il)
        {
            var config = _configuration.Get(_variable.OwnerType);
            var comparisons = _membersProvider
                .GetMembers(_variable.VariableType)
                .Select(_resolver.GetHasher);

            il.LoadLong(config.HashSeed)
              .Store(typeof(long), out var hash);

            foreach (var item in comparisons) {
                using (il.LocalsScope()) {
                    var add = Add(
                        ShiftLeft(LoadLocal(hash), LoadInteger(5)),
                        LoadLocal(hash));

                    il.Xor(add, Execute(item.Emit, Cast(typeof(long))))
                      .Store(hash);
                }
            }

            return il.LoadLocal(hash).Cast(typeof(int));
        }
    }
}