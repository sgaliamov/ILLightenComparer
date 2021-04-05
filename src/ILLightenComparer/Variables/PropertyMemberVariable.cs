﻿using System;
using System.Reflection;
using Illuminator;
using Illuminator.Extensions;

namespace ILLightenComparer.Variables
{
    internal sealed class PropertyMemberVariable : IVariable
    {
        private readonly PropertyInfo _propertyInfo;

        private PropertyMemberVariable(PropertyInfo propertyInfo) => _propertyInfo = propertyInfo;

        public Type OwnerType => _propertyInfo.DeclaringType;
        public Type VariableType => _propertyInfo.PropertyType;

        public ILEmitter Load(ILEmitter il, ushort arg)
        {
            if (OwnerType.IsValueType) {
                il.LoadArgumentAddress(arg);
            } else {
                il.LoadArgument(arg);
            }

            return il.CallMethod(_propertyInfo.GetMethod);
        }

        public ILEmitter LoadAddress(ILEmitter il, ushort arg) => Load(il, arg)
            .Stloc(VariableType.GetUnderlyingType(), out var local)
            .Ldloca(local);

        public static IVariable Create(MemberInfo memberInfo) =>
            memberInfo is PropertyInfo info
                ? new PropertyMemberVariable(info)
                : null;
    }
}
