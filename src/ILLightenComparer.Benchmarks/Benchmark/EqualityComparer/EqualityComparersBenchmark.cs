using System;
using System.Collections.Generic;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Benchmarks.Benchmark.EqualityComparer
{
    [MedianColumn]
    public abstract class EqualityComparersBenchmark<T>
    {
        private const int N = 100000;

        private readonly IFixture _fixture = FixtureBuilder.GetInstance();
        private readonly IEqualityComparer<T> _il;
        private readonly IEqualityComparer<T> _manual;
        private readonly IEqualityComparer<T> _nito;

        private readonly T[] _one = new T[N];
        private readonly T[] _other = new T[N];

        // ReSharper disable once NotAccessedField.Local
        private bool _out;

        protected EqualityComparersBenchmark(IEqualityComparer<T> manual, IEqualityComparer<T> il, IEqualityComparer<T> nito)
        {
            _manual = manual;
            _il = il;
            _nito = nito;
        }

        [Benchmark(Description = "IL Lighten Comparer")]
        public void IL_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _il.Equals(_one[i], _other[i]);
            }
        }

        [Benchmark(Baseline = true, Description = "Manual implementation")]
        public void Manual_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _manual.Equals(_one[i], _other[i]);
            }
        }

        [Benchmark(Description = "Nito Comparer")]
        public void Nito_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _nito.Equals(_one[i], _other[i]);
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            for (var i = 0; i < N; i++) {
                _one[i] = _fixture.Create<T>();
                _other[i] = _fixture.Create<T>();

                var compare = _manual.Equals(_one[i], _other[i]);

                if (compare != _il.Equals(_one[i], _other[i])) {
                    throw new InvalidOperationException("Light comparer is broken.");
                }

                if (compare != _nito.Equals(_one[i], _other[i])) {
                    throw new InvalidOperationException("Nito comparer is broken.");
                }
            }
        }
    }
}
