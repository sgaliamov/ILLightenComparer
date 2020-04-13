﻿using System.Collections.Generic;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static Illuminator.Functional;

namespace ILLightenComparer.Equality.Hashers
{
    internal sealed class ArrayHasher : IHasherEmitter
    {
        private readonly IConfigurationProvider _configuration;
        private readonly IVariable _variable;
        private readonly HasherResolver _resolver;

        private ArrayHasher(HasherResolver resolver, IConfigurationProvider configuration, IVariable variable)
        {
            _resolver = resolver;
            _configuration = configuration;
            _variable = variable;
        }

        public static ArrayHasher Create(HasherResolver resolver, IConfigurationProvider configuration, IVariable variable)
        {
            var variableType = variable.VariableType;
            if (variableType.IsArray && variableType.GetArrayRank() == 1) {
                return new ArrayHasher(resolver, configuration, variable);
            }

            return null;
        }

        public ILEmitter Emit(ILEmitter il)
        {
            var arrayType = _variable.VariableType;
            var ownerType = _variable.OwnerType;
            var config = _configuration.Get(ownerType);

            il.LoadLong(config.HashSeed) // start hash
              .Store(typeof(long), out var hash)
              .Execute(_variable.Load(Arg.Input)) // load array
              .Store(arrayType, out var array)
              .LoadInteger(0) // start loop
              .Store(typeof(int), out var index)
              .EmitArrayLength(arrayType, array, out var count)
              .DefineLabel(out var loopStart);

            // todo: 1. sort when IgnoreCollectionOrder

            using (il.LocalsScope()) {
                il.MarkLabel(loopStart)
                  .IfNotEqual_Un_S(LoadLocal(index), LoadLocal(count), out var next)
                  .Return(hash)
                  .MarkLabel(next);
            }

            using (il.LocalsScope()) {
                var arrays = new Dictionary<ushort, LocalBuilder>(2) { [Arg.X] = array };
                var itemVariable = new ArrayItemVariable(arrayType, ownerType, arrays, index);
                var itemHasher = _resolver.GetHasherEmitter(itemVariable);

                il.EmitHashing(hash, itemHasher.Emit)
                  .Add(LoadLocal(index), LoadInteger(1))
                  .Store(index)
                  .GoTo(loopStart);
            }

            return il;
        }
    }
}
