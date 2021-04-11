using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static Illuminator.Functions;

namespace ILLightenComparer.Shared.Comparisons
{
    internal sealed class ArrayComparisonEmitter
    {
        private readonly EmitCheckIfLoopsAreDoneDelegate _emitCheckIfLoopsAreDone;
        private readonly EmitReferenceComparisonDelegate _emitReferenceComparison;
        private readonly IResolver _resolver;

        public ArrayComparisonEmitter(
            IResolver resolver,
            EmitCheckIfLoopsAreDoneDelegate emitCheckIfLoopsAreDone,
            EmitReferenceComparisonDelegate emitReferenceComparison)
        {
            _resolver = resolver;
            _emitCheckIfLoopsAreDone = emitCheckIfLoopsAreDone;
            _emitReferenceComparison = emitReferenceComparison;
        }

        public ILEmitter EmitCompareArrays(
            ILEmitter il,
            Type arrayType,
            Type ownerType,
            LocalBuilder xArray,
            LocalBuilder yArray,
            Label afterLoop)
        {
            // todo: 2. compare array lengths at the beginning
            il.EmitArrayLength(arrayType, xArray, out var countX)
              .EmitArrayLength(arrayType, yArray, out var countY)
              .Ldc_I4(0)
              .Stloc(typeof(int), out var index)
              .DefineLabel(out var loopStart)
              .DefineLabel(out var continueLoop)
              .MarkLabel(loopStart);

            using (il.LocalsScope()) {
                il.Ceq(Ldloc(index), Ldloc(countX), out var isDoneX)
                  .Ceq(Ldloc(index), Ldloc(countY), out var isDoneY);
                _emitCheckIfLoopsAreDone(il, isDoneX, isDoneY, afterLoop);
            }

            using (il.LocalsScope()) {
                var arrays = new Dictionary<ushort, LocalBuilder>(2) {
                    [Arg.X] = xArray,
                    [Arg.Y] = yArray
                };
                var itemVariable = new ArrayItemVariable(arrayType, ownerType, arrays, index);
                var itemComparison = _resolver.GetComparisonEmitter(itemVariable);

                return il.Emit(
                             itemComparison.Emit(continueLoop),
                             itemComparison.EmitCheckForResult(continueLoop))
                         .MarkLabel(continueLoop)
                         .Add(Ldloc(index), Ldc_I4(1))
                         .Stloc(index)
                         .Br(loopStart);
            }
        }

        public (LocalBuilder collectionX, LocalBuilder collectionY) EmitLoad(IVariable variable, ILEmitter il, Label gotoNext)
        {
            variable.Load(il, Arg.X).Stloc(variable.VariableType, out var collectionX);
            variable.Load(il, Arg.Y).Stloc(variable.VariableType, out var collectionY);

            if (!variable.VariableType.IsValueType) {
                _emitReferenceComparison(il, Ldloc(collectionX), Ldloc(collectionY), Br(gotoNext)); // need, because a collection can be a member of an object
            }

            return (collectionX, collectionY);
        }
    }
}
