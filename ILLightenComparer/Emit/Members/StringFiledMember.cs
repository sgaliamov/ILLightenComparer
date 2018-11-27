﻿using System.Reflection;
using ILLightenComparer.Emit.Emitters;
using ILLightenComparer.Emit.Emitters.Acceptors;
using ILLightenComparer.Emit.Emitters.Members;

namespace ILLightenComparer.Emit.Members
{
    internal sealed class StringFiledMember : FieldMember, IStringAcceptor, IFieldValues
    {
        public StringFiledMember(FieldInfo fieldInfo) : base(fieldInfo) { }

        public ILEmitter Accept(StackEmitter visitor, ILEmitter il) => visitor.Visit(this, il);
        public ILEmitter Accept(CompareEmitter visitor, ILEmitter il) => visitor.Visit(this, il);
    }
}
