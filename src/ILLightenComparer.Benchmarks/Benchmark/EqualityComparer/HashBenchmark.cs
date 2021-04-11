using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Benchmarks.Benchmark.EqualityComparer
{
    [MedianColumn]
    public abstract class HashBenchmark<T>
    {
        private const int N = 100000;

        private readonly IFixture _fixture = FixtureBuilder.GetSimpleInstance();
        private readonly IEqualityComparer<T> _il;
        private readonly IEqualityComparer<T> _manual;
        private readonly IEqualityComparer<T> _nito;

        private readonly T[] _one = new T[N];

        [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private int _out;

        protected HashBenchmark(IEqualityComparer<T> manual, IEqualityComparer<T> il, IEqualityComparer<T> nito)
        {
            _manual = manual;
            _il = il;
            _nito = nito;
        }

        [Benchmark(Description = "IL Lighten Comparer")]
        public void IL_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _il.GetHashCode(_one[i]);
            }
        }

        [Benchmark(Baseline = true, Description = "Manual implementation")]
        public void Manual_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _manual.GetHashCode(_one[i]);
            }
        }

        [Benchmark(Description = "Nito Comparer")]
        public void Nito_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _nito.GetHashCode(_one[i]);
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            for (var i = 0; i < N; i++) {
                _one[i] = _fixture.Create<T>();
                _out = _manual.GetHashCode(_one[i]);
            }
        }
    }
}
