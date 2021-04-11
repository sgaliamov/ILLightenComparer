using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Shared;

namespace ILLightenComparer.Comparer
{
    internal sealed class ComparerContext : IComparerContext
    {
        private const string CompareMethodName = nameof(IComparer.Compare);
        private readonly IConfigurationProvider _configuration;
        private readonly ComparersCollection _emittedComparers = new();
        private readonly GenericProvider _genericProvider;

        public ComparerContext(MembersProvider membersProvider, IConfigurationProvider configuration)
        {
            _configuration = configuration;

            var resolver = new ComparisonResolver(this, membersProvider, _configuration);

            var methodEmitters = new Dictionary<string, IStaticMethodEmitter> {
                [CompareMethodName] = new CompareStaticMethodEmitter(resolver)
            };

            _genericProvider = new GenericProvider(typeof(IComparer<>), new GenericTypeBuilder(methodEmitters, _configuration));
        }

        private IComparer<T> CreateInstance<T>(Type key) =>
            _genericProvider
                .EnsureComparerType(key)
                .CreateInstance<IComparerContext, IComparer<T>>(this);

        public int DelayedCompare<T>(T x, T y, CycleDetectionSet xSet, CycleDetectionSet ySet)
        {
            var comparer = _configuration.GetCustomComparer<T>();
            if (comparer != null) {
                return comparer.Compare(x, y);
            }

            if (!typeof(T).IsValueType) {
                if (x == null) {
                    return y == null ? 0 : -1;
                }

                if (y == null) {
                    return 1;
                }
            }

            var xType = x.GetType();
            var yType = y.GetType();
            if (xType != yType) {
                throw new ArgumentException($"Argument types {xType} and {yType} are not matched.");
            }

            if (xType == typeof(object)) {
                return 0;
            }

            var compareMethod = GetCompiledStaticCompareMethod(xType);

            return compareMethod.InvokeCompare<IComparerContext, T, int>(xType, this, x, y, xSet, ySet);
        }

        public IComparer<T> GetComparer<T>() =>
            _configuration.GetCustomComparer<T>()
            ?? (IComparer<T>)_emittedComparers
                .GetOrAdd(typeof(T), CreateInstance<T>);

        public MethodInfo GetCompiledStaticCompareMethod(Type type) =>
            _genericProvider.GetCompiledStaticMethod(type, CompareMethodName);

        public MethodInfo GetStaticCompareMethodInfo(Type type) =>
            _genericProvider.GetStaticMethodInfo(type, CompareMethodName);
    }

    internal interface IComparerContext : IComparerProvider, IContext
    {
        int DelayedCompare<T>(T x, T y, CycleDetectionSet xSet, CycleDetectionSet ySet);
    }
}
