using System.Reflection.Emit;
using Illuminator;

namespace ILLightenComparer.Abstractions
{
    internal interface IComparisonEmitter
    {
        /// <summary>
        ///     Compare and leave a result in the stack.
        /// </summary>
        ILEmitter Emit(ILEmitter il, Label next);

        /// <summary>
        ///     Does the comparison puts a result into the stack.
        ///     Comparisons with many items does not do it.
        /// </summary>
        ILEmitter EmitCheckForResult(ILEmitter il, Label next);
    }

    internal static class ComparisonEmitterExtensions
    {
        public static ILEmitterFunc Emit(this IComparisonEmitter emitter, Label next) =>
            (in ILEmitter il) => emitter.Emit(il, next);

        public static ILEmitterFunc EmitCheckForResult(this IComparisonEmitter emitter, Label next) =>
            (in ILEmitter il) => emitter.EmitCheckForResult(il, next);
    }
}
