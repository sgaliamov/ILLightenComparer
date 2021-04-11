using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using ILLightenComparer.Extensions;
using Illuminator;
using static ILLightenComparer.Extensions.Functions;
using static Illuminator.Functions;

namespace ILLightenComparer.Shared
{
    internal sealed class CycleDetectionSet : ConcurrentDictionary<object, byte>
    {
        public static readonly ConstructorInfo DefaultConstructor = typeof(CycleDetectionSet).GetConstructor(Type.EmptyTypes);

        internal static readonly MethodInfo GetCountProperty = typeof(CycleDetectionSet).GetProperty(nameof(Count))!.GetGetMethod();

        private static readonly MethodInfo RemoveMethod = typeof(IDictionary).FindMethod(
            nameof(IDictionary.Remove),
            new[] { typeof(object) });

        private static readonly MethodInfo TryAddMethod = typeof(CycleDetectionSet).GetMethod(
            nameof(ConcurrentDictionary<object, byte>.TryAdd),
            new[] { typeof(object), typeof(byte) });

        public static ILEmitterFunc Remove(short set, short arg, Type argType) =>
            CallMethod(
                RemoveMethod,
                LoadArgument(set),
                LoadArgument(arg) + EmitIf(argType.IsValueType, Box((in ILEmitter il) => il, argType)));

        public static ILEmitterFunc TryAdd(short set, short arg, Type argType) =>
            CallMethod(
                TryAddMethod,
                LoadArgument(set),
                LoadArgument(arg),
                EmitIf(argType.IsValueType, Box((in ILEmitter il) => il, argType)),
                Ldc_I4(0));

        public static ILEmitterFunc GetCount(short arg) =>
            CallMethod(GetCountProperty, LoadArgument(arg));
    }
}
