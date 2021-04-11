using System;
using System.Reflection;
using ILLightenComparer.Shared;

namespace ILLightenComparer.Extensions
{
    internal static class Methods
    {
        public static readonly ConstructorInfo ArgumentExceptionConstructor = typeof(ArgumentException).GetConstructor(new[] { typeof(string) });

        public static TResult InvokeCompare<TContext, TComparable, TResult>(
            this MethodInfo method,
            Type actualType,
            TContext context,
            TComparable x,
            TComparable y,
            CycleDetectionSet xSet,
            CycleDetectionSet ySet)
        {
            var isDeclaringTypeMatchedActualMemberType = typeof(TComparable) == actualType;
            if (!isDeclaringTypeMatchedActualMemberType) {
                // todo: 1. cache delegates and benchmark ways:
                // - direct Invoke;
                // - DynamicInvoke;
                // var genericType = typeof(Method.StaticMethodDelegate<>).MakeGenericType(type);
                // var @delegate = compareMethod.CreateDelegate(genericType);
                // return (int)@delegate.DynamicInvoke(this, x, y, hash);
                // - DynamicMethod;
                // - generate static class wrapper.

                return (TResult)method.Invoke(
                    null,
                    new object[] { context, x, y, xSet, ySet });
            }

            var compare = method.CreateDelegate<Func<TContext, TComparable, TComparable, CycleDetectionSet, CycleDetectionSet, TResult>>();

            return compare(context, x, y, xSet, ySet);
        }
    }
}
