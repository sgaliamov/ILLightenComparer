using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static Illuminator.Functions;

namespace ILLightenComparer.Equality.Hashers
{
    internal sealed class ArrayHashEmitter
    {
        private readonly HasherResolver _resolver;
        private readonly IVariable _variable;

        public ArrayHashEmitter(HasherResolver resolver, IVariable variable)
        {
            _resolver = resolver;
            _variable = variable;
        }

        public ILEmitter Emit(ILEmitter il, Type arrayType, LocalBuilder array, LocalBuilder hash)
        {
            il.Ldc_I4(0) // start loop
              .Stloc(typeof(int), out var index)
              .EmitArrayLength(arrayType, array, out var count)
              .DefineLabel(out var loopStart)
              .DefineLabel(out var loopEnd);

            using (il.LocalsScope()) {
                il.MarkLabel(loopStart)
                  .Bne_Un_S(Ldloc(index), Ldloc(count), out var next)
                  .Br(loopEnd)
                  .MarkLabel(next);
            }

            using (il.LocalsScope()) {
                var arrays = new Dictionary<ushort, LocalBuilder>(1) { [Arg.Input] = array };
                var itemVariable = new ArrayItemVariable(arrayType, _variable.OwnerType, arrays, index);

                _resolver
                    .GetHasherEmitter(itemVariable)
                    .EmitHashing(il, hash)
                    .Add(Ldloc(index), Ldc_I4(1))
                    .Stloc(index)
                    .Br(loopStart);
            }

            return il.MarkLabel(loopEnd).Ldloc(hash);
        }
    }
}
