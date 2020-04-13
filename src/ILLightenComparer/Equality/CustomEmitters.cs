﻿using System;
using System.Reflection.Emit;
using Illuminator;

namespace ILLightenComparer.Equality
{
    internal static class CustomEmitters
    {
        public static ILEmitter EmitReturnIfFalsy(this ILEmitter il, Label next) => il.IfTrue(next).Return(0);

        public static ILEmitter EmitCheckIfLoopsAreDone(this ILEmitter il, LocalBuilder isDoneX, LocalBuilder isDoneY, Label gotoNext) => il
            .LoadLocal(isDoneX)
            .IfFalse_S(out var checkIsDoneY)
            .LoadLocal(isDoneY)
            .IfFalse_S(out var returnFalse)
            .GoTo(gotoNext)
            .MarkLabel(returnFalse)
            .Return(0)
            .MarkLabel(checkIsDoneY)
            .LoadLocal(isDoneY)
            .IfFalse_S(out var next)
            .Return(0)
            .MarkLabel(next);

        public static ILEmitter EmitReferenceComparison(this ILEmitter il, Func<ILEmitter, ILEmitter> loadX, Func<ILEmitter, ILEmitter> loadY, Func<ILEmitter, ILEmitter> ifEqual) => il
            .Execute(loadX)
            .Execute(loadY)
            .IfNotEqual_Un_S(out var checkX)
            .Execute(ifEqual)
            .MarkLabel(checkX)
            .Execute(loadX)
            .IfTrue_S(out var checkY)
            .Return(0)
            .MarkLabel(checkY)
            .Execute(loadY)
            .IfTrue_S(out var next)
            .Return(0)
            .MarkLabel(next);
    }
}