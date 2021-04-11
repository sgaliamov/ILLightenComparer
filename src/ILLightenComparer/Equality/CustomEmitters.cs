using System;
using System.Reflection.Emit;
using ILLightenComparer.Extensions;
using Illuminator;

namespace ILLightenComparer.Equality
{
    internal static class CustomEmitters
    {
        /// <summary>
        ///     Returns zero if stack has zero value, otherwise got to <paramref name="next" />.
        /// </summary>
        public static ILEmitter EmitReturnIfFalsy(this ILEmitter il, Label next) =>
            il.Brtrue(next)
              .Ret(0);

        public static ILEmitter EmitCheckIfLoopsAreDone(this ILEmitter il, LocalBuilder isDoneX, LocalBuilder isDoneY, Label gotoNext) =>
            il.Ldloc(isDoneX)
              .Brfalse_S(out var checkIsDoneY)
              .Ldloc(isDoneY)
              .Brfalse_S(out var returnFalse)
              .Br(gotoNext)
              .MarkLabel(returnFalse)
              .Ret(0)
              .MarkLabel(checkIsDoneY)
              .Ldloc(isDoneY)
              .Brfalse_S(out var next)
              .Ret(0)
              .MarkLabel(next);

        public static ILEmitter EmitReferenceComparison(this ILEmitter il, ILEmitterFunc loadX, ILEmitterFunc loadY, ILEmitterFunc ifEqual) =>
            il.Bne_Un_S(loadX, loadY, out var checkX)
              .Emit(ifEqual)
              .MarkLabel(checkX)
              .Emit(loadX)
              .Brtrue_S(out var checkY)
              .Ret(0)
              .MarkLabel(checkY)
              .Emit(loadY)
              .Brtrue_S(out var next)
              .Ret(0)
              .MarkLabel(next);

        public static ILEmitter EmitCheckNullablesForValue(this ILEmitter il, ILEmitterFunc nullableX, ILEmitterFunc nullableY, Type nullableType, Label ifBothNull)
        {
            var hasValueMethod = nullableType.GetPropertyGetter("HasValue");

            return il.CallMethod(hasValueMethod, nullableY)
                     .Stloc(typeof(bool), out var secondHasValue)
                     .CallMethod(hasValueMethod, nullableX)
                     .Brtrue_S(out var ifFirstHasValue)
                     .Ldloc(secondHasValue)
                     .Brfalse(ifBothNull)
                     .Ret(0)
                     .MarkLabel(ifFirstHasValue)
                     .Ldloc(secondHasValue)
                     .Brtrue_S(out var next)
                     .Ret(0)
                     .MarkLabel(next);
        }
    }
}
