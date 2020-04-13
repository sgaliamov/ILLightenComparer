﻿using System.Reflection.Emit;
using Illuminator;

namespace ILLightenComparer.Abstractions
{
    public delegate ILEmitter EmitCheckIfLoopsAreDoneDelegate(ILEmitter il, LocalBuilder index, LocalBuilder countX, LocalBuilder countY, Label afterLoop);

    public delegate ILEmitter EmitterDelegate(ILEmitter il, Label next);
}