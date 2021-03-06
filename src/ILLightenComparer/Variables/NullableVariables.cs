﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Extensions;
using Illuminator;

namespace ILLightenComparer.Variables
{
    internal sealed class NullableVariables : IVariable
    {
        private readonly MethodInfo _getValueMethod;
        private readonly IReadOnlyDictionary<int, LocalBuilder> _nullables;

        public NullableVariables(Type variableType, Type ownerType, IReadOnlyDictionary<int, LocalBuilder> nullables)
        {
            Debug.Assert(variableType.IsNullable());

            OwnerType = ownerType;
            VariableType = variableType.GetUnderlyingType();

            _nullables = nullables;
            _getValueMethod = variableType.GetPropertyGetter("Value");
        }

        public Type VariableType { get; }
        public Type OwnerType { get; }

        public ILEmitter Load(ILEmitter il, ushort arg) =>
            il.LoadLocalAddress(_nullables[arg])
              .CallMethod(_getValueMethod);

        public ILEmitter LoadAddress(ILEmitter il, ushort arg)
        {
            var underlyingType = VariableType.GetUnderlyingType();

            return il.LoadLocalAddress(_nullables[arg])
                     .CallMethod(_getValueMethod)
                     .Stloc(underlyingType, out var x)
                     .LoadLocalAddress(x);
        }
    }
}
