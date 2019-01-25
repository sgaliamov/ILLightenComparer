﻿using ILLightenComparer.Emit.Shared;

namespace ILLightenComparer.Emit.v2.Sources
{
    internal sealed class Arrays : ISource
    {
        public ILEmitter Accept(CompareEmitter visitor, ILEmitter il)
        {
            return visitor.Visit(this, il);
        }
    }
}
