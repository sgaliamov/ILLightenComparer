﻿using System;
using ILLightenComparer.Config;
using ILLightenComparer.Emitters.Builders;
using ILLightenComparer.Reflection;
using ILLightenComparer.Shared;
using Illuminator.Extensions;

namespace ILLightenComparer.Emitters
{
    internal sealed class Context : IContext
    {
        private readonly IConfigurationProvider _configurations;

        public Context(IConfigurationProvider configurations)
        {
            _configurations = configurations;
            Builder = new ContextBuilder(this, configurations);
        }

        public ContextBuilder Builder { get; }

        public int DelayedCompare<T>(T x, T y, ConcurrentSet<object> xSet, ConcurrentSet<object> ySet)
        {
            var comparer = _configurations.GetCustomComparer<T>();
            if (comparer != null) {
                return comparer.Compare(x, y);
            }

            if (!typeof(T).IsValueType) {
                if (x == null) { return y == null ? 0 : -1; }

                if (y == null) { return 1; }
            }

            var xType = x.GetType();
            var yType = y.GetType();
            if (xType != yType) {
                throw new ArgumentException($"Argument types {xType} and {yType} are not matched.");
            }

            return Compare(xType, x, y, xSet, ySet);
        }

        private int Compare<T>(Type type, T x, T y, ConcurrentSet<object> xSet, ConcurrentSet<object> ySet)
        {
            var compareMethod = Builder.GetCompiledStaticCompareMethod(type);

            var isDeclaringTypeMatchedActualMemberType = typeof(T) == type;
            if (!isDeclaringTypeMatchedActualMemberType) {
                // todo: cache delegates and benchmark ways:
                // - direct Invoke;
                // - DynamicInvoke;
                // var genericType = typeof(Method.StaticMethodDelegate<>).MakeGenericType(type);
                // var @delegate = compareMethod.CreateDelegate(genericType);
                // return (int)@delegate.DynamicInvoke(this, x, y, hash);
                // - DynamicMethod;
                // - generate static class wrapper.

                return (int)compareMethod.Invoke(
                    null,
                    new object[] { this, x, y, xSet, ySet });
            }

            var compare = compareMethod.CreateDelegate<Method.StaticMethodDelegate<T>>();

            return compare(this, x, y, xSet, ySet);
        }
    }

    internal interface IContext
    {
        int DelayedCompare<T>(T x, T y, ConcurrentSet<object> xSet, ConcurrentSet<object> ySet);
    }
}
