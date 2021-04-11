using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Extensions;
using Illuminator;
using static ILLightenComparer.Extensions.Functions;

namespace ILLightenComparer.Variables
{
    internal sealed class EnumerableItemVariable : IVariable
    {
        private readonly IReadOnlyDictionary<ushort, LocalBuilder> _enumerators;
        private readonly MethodInfo _getCurrentMethod;

        public EnumerableItemVariable(Type ownerType, Type elementType, MethodInfo getCurrentMethod, IReadOnlyDictionary<ushort, LocalBuilder> enumerators)
        {
            OwnerType = ownerType;
            VariableType = elementType;

            _enumerators = enumerators;
            _getCurrentMethod = getCurrentMethod;
        }

        /// <summary>
        ///     Element type.
        /// </summary>
        public Type VariableType { get; }

        /// <summary>
        ///     Enumerator type.
        /// </summary>
        public Type OwnerType { get; }

        public ILEmitter Load(ILEmitter il, ushort arg) => il.CallMethod(_getCurrentMethod, LoadCaller(_enumerators[arg]));

        public ILEmitter LoadAddress(ILEmitter il, ushort arg) => il
                                                                  .CallMethod(_getCurrentMethod, LoadCaller(_enumerators[arg]))
                                                                  .Stloc(VariableType, out var local)
                                                                  .LoadLocalAddress(local);
    }
}
