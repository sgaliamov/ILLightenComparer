﻿using System.Reflection;
using ILLightenComparer.Emit.Emitters;
using ILLightenComparer.Emit.Emitters.Acceptors;
using ILLightenComparer.Emit.Emitters.Members;

namespace ILLightenComparer.Emit.Members
{
    internal sealed class StringFieldMember : FieldMember, IStringAcceptor, ITwoArgumentsField
    {
        private StringFieldMember(FieldInfo fieldInfo) : base(fieldInfo) { }

        public ILEmitter LoadMembers(StackEmitter visitor, ILEmitter il) => visitor.Visit(this, il);
        public ILEmitter Accept(CompareEmitter visitor, ILEmitter il) => visitor.Visit(this, il);

        public static StringFieldMember Create(MemberInfo memberInfo) =>
            memberInfo is FieldInfo info && info.FieldType == typeof(string)
                ? new StringFieldMember(info)
                : null;
    }

    internal sealed class StringPropertyMember : PropertyMember, IStringAcceptor, ITwoArgumentsProperty
    {
        private StringPropertyMember(PropertyInfo propertyInfo) : base(propertyInfo) { }

        public ILEmitter LoadMembers(StackEmitter visitor, ILEmitter il) => visitor.Visit(this, il);
        public ILEmitter Accept(CompareEmitter visitor, ILEmitter il) => visitor.Visit(this, il);

        public static StringPropertyMember Create(MemberInfo memberInfo) =>
            memberInfo is PropertyInfo info && info.PropertyType == typeof(string)
                ? new StringPropertyMember(info)
                : null;
    }
}
