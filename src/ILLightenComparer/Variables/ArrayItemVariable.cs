using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Extensions;
using Illuminator;
using static Illuminator.Functions;

namespace ILLightenComparer.Variables
{
    internal sealed class ArrayItemVariable : IVariable
    {
        private const string GetMethodName = "Get";
        private readonly IReadOnlyDictionary<ushort, LocalBuilder> _arrays;
        private readonly MethodInfo _getItemMethod;
        private readonly LocalBuilder _indexVariable;

        public ArrayItemVariable(
            Type arrayType,
            Type ownerType,
            IReadOnlyDictionary<ushort, LocalBuilder> arrays,
            LocalBuilder indexVariable)
        {
            Debug.Assert(arrayType != null);

            _getItemMethod = arrayType.GetMethod(GetMethodName, new[] { typeof(int) }) ?? throw new ArgumentException(nameof(arrayType));
            _arrays = arrays;
            _indexVariable = indexVariable ?? throw new ArgumentNullException(nameof(indexVariable));

            VariableType = arrayType.GetElementType();
            OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
        }

        public Type VariableType { get; }
        public Type OwnerType { get; }

        public ILEmitter Load(ILEmitter il, ushort arg) =>
            il.CallMethod(
                _getItemMethod,
                Ldloc(_arrays[arg]),
                Ldloc(_indexVariable));

        public ILEmitter LoadAddress(ILEmitter il, ushort arg) =>
            Load(il, arg)
                .Stloc(VariableType, out var local)
                .LoadLocalAddress(local);
    }
}
