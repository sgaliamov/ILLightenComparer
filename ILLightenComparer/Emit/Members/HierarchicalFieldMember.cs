﻿using System.Reflection;
using ILLightenComparer.Emit.Emitters;
using ILLightenComparer.Emit.Emitters.Acceptors;
using ILLightenComparer.Emit.Emitters.Members;

namespace ILLightenComparer.Emit.Members
{
    internal sealed class HierarchicalFieldMember : FieldMember, IHierarchicalAcceptor, ITwoArgumentsField
    {
        public HierarchicalFieldMember(FieldInfo fieldInfo) : base(fieldInfo) { }

        public ILEmitter LoadArguments(StackEmitter visitor, ILEmitter il) => visitor.Visit(this, il);
        public ILEmitter Accept(CompareEmitter visitor, ILEmitter il) => visitor.Visit(this, il);
    }
}
