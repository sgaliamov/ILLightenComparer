﻿using System.Collections.Generic;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Variables;
using Illuminator;
using Illuminator.Extensions;
using static ILLightenComparer.Extensions.Functions;

namespace ILLightenComparer.Shared.Comparisons
{
    internal sealed class NullableComparison : IComparisonEmitter
    {
        private readonly IResolver _resolver;
        private readonly EmitterDelegate _checkForIntermediateResultEmitter;
        private readonly EmitCheckNullablesForValueDelegate _emitCheckNullablesForValue;
        private readonly IVariable _variable;

        private NullableComparison(
            IResolver resolver,
            EmitterDelegate checkForIntermediateResultEmitter,
            EmitCheckNullablesForValueDelegate emitCheckNullablesForValue,
            IVariable variable)
        {
            _resolver = resolver;
            _checkForIntermediateResultEmitter = checkForIntermediateResultEmitter;
            _emitCheckNullablesForValue = emitCheckNullablesForValue;
            _variable = variable;
        }

        public static NullableComparison Create(
            IResolver resolver,
            EmitterDelegate checkForIntermediateResultEmitter,
            EmitCheckNullablesForValueDelegate emitCheckNullablesForValue,
            IVariable variable)
        {
            if (variable.VariableType.IsNullable()) {
                return new NullableComparison(resolver, checkForIntermediateResultEmitter, emitCheckNullablesForValue, variable);
            }

            return null;
        }

        public ILEmitter Emit(ILEmitter il, Label gotoNext)
        {
            var variableType = _variable.VariableType;

            _variable.Load(il, Arg.X).Stloc(variableType, out var nullableX);
            _variable.Load(il, Arg.Y).Stloc(variableType, out var nullableY);

            var isMember = !(_variable is ArgumentVariable);
            if (isMember) {
                _emitCheckNullablesForValue(il, Ldloca(nullableX), Ldloca(nullableY), variableType, gotoNext);
            }

            var nullableVariables = new NullableVariables(variableType, _variable.OwnerType, new Dictionary<ushort, LocalBuilder>(2) {
                [Arg.X] = nullableX,
                [Arg.Y] = nullableY
            });

            return _resolver
                .GetComparisonEmitter(nullableVariables)
                .Emit(il, gotoNext);
        }

        public ILEmitter EmitCheckForResult(ILEmitter il, Label next) => _checkForIntermediateResultEmitter(il, next);
    }
}
