using System.Reflection;
using System.Reflection.Emit;
using Illuminator;

namespace ILLightenComparer.Extensions
{
    internal static class Functions
    {
        public static ILEmitterFunc Id() =>
            (in ILEmitter il) => il;

        public static ILEmitterFunc CallMethod(MethodInfo methodInfo, params ILEmitterFunc[] funcs) =>
            (in ILEmitter il) => il.CallMethod(methodInfo, funcs);

        public static ILEmitterFunc Cast<T>(ILEmitterFunc value) =>
            (in ILEmitter il) => il.Cast<T>(value);

        public static ILEmitterFunc EmitIf(bool condition, params ILEmitterFunc[] actions) =>
            (in ILEmitter il) => il.EmitIf(condition, actions);

        public static ILEmitterFunc If(ILEmitterFunc action, ILEmitterFunc whenTrue, ILEmitterFunc elseAction) =>
            (in ILEmitter il) => il.If(action, whenTrue, elseAction);

        public static ILEmitterFunc If(ILEmitterFunc action, ILEmitterFunc whenTrue) =>
            (in ILEmitter il) => il.If(action, whenTrue);

        public static ILEmitterFunc LoadLocalAddress(LocalBuilder local) =>
            (in ILEmitter il) => il.LoadLocalAddress(local);

        public static ILEmitterFunc LoadArgument(int argumentIndex) =>
            (in ILEmitter il) => il.LoadArgument(argumentIndex);

        public static ILEmitterFunc LoadArgumentAddress(ushort argumentIndex) =>
            (in ILEmitter il) => il.LoadArgumentAddress(argumentIndex);

        public static ILEmitterFunc LoadCaller(LocalBuilder local) =>
            (in ILEmitter il) => il.LoadCaller(local);

        public static ILEmitterFunc Ret(int value) =>
            (in ILEmitter il) => il.Ret(value);

        public static ILEmitterFunc Ret(LocalBuilder local) =>
            (in ILEmitter il) => il.Ret(local);
    }
}
