﻿using System;
using ILLightenComparer.Emit.Shared;
using ILLightenComparer.Emit.v2.Visitors;

namespace ILLightenComparer.Emit.v2.Variables
{
    /// <summary>
    ///     Arguments of the compare function.
    /// </summary>
    internal sealed class ArgumentVariable : IVariable
    {
        public ArgumentVariable(Type variableType)
        {
            VariableType = variableType;
        }

        public Type VariableType { get; }

        public ILEmitter Load(VariableLoader visitor, ILEmitter il, ushort arg)
        {
            return visitor.Load(this, il, arg);
        }

        public ILEmitter LoadAddress(VariableLoader visitor, ILEmitter il, ushort arg)
        {
            return visitor.LoadAddress(this, il, arg);
        }
    }
}