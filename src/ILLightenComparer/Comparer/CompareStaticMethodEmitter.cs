using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static ILLightenComparer.Shared.CycleDetectionSet;
using static ILLightenComparer.Extensions.Functions;
using static Illuminator.Functions;

namespace ILLightenComparer.Comparer
{
    internal sealed class CompareStaticMethodEmitter : IStaticMethodEmitter
    {
        private static void EmitCycleDetection(ILEmitter il, Type objectType) =>
            il.Ceq(Ldc_I4(0),
                   Or(TryAdd(Arg.SetX, Arg.X, objectType),
                      TryAdd(Arg.SetY, Arg.Y, objectType)))
              .Brfalse_S(out var next)
              .Ret(Sub(GetCount(Arg.SetX), GetCount(Arg.SetY)))
              .MarkLabel(next);

        private readonly ComparisonResolver _resolver;

        public CompareStaticMethodEmitter(ComparisonResolver resolver)
        {
            _resolver = resolver;
        }

        public void Build(Type objectType, bool detectCycles, MethodBuilder staticMethodBuilder)
        {
            using var il = staticMethodBuilder.CreateILEmitter();

            il.DefineLabel(out var exit);

            var needReferenceComparison =
                !objectType.IsComparable() // ComparablesComparison do this check
                && !objectType.ImplementsGenericInterface(typeof(IEnumerable<>)); // collections do reference comparisons anyway

            if (needReferenceComparison) {
                if (!objectType.IsValueType) {
                    il.EmitReferenceComparison(LoadArgument(Arg.X), LoadArgument(Arg.Y), Ret(0));
                } else if (objectType.IsNullable()) {
                    il.EmitCheckNullablesForValue(LoadArgumentAddress(Arg.X), LoadArgumentAddress(Arg.Y), objectType, exit);
                }
            }

            if (detectCycles) {
                EmitCycleDetection(il, objectType);
            }

            var emitter = _resolver.GetComparisonEmitter(new ArgumentVariable(objectType));

            emitter.Emit(il, exit);

            if (detectCycles) {
                il.Emit(Remove(Arg.SetX, Arg.X, objectType))
                  .Emit(Remove(Arg.SetY, Arg.Y, objectType));
            }

            il.Emit(emitter.EmitCheckForResult(exit))
              .MarkLabel(exit)
              .Ret(0);
        }

        // no need detect cycle as flow goes outside context
        public bool NeedCreateCycleDetectionSets(Type objectType) => !objectType.IsComparable();
    }
}
