using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Benchmarks.Benchmark.Comparer
{
    [MedianColumn]
    public abstract class ComparersBenchmark<T>
    {
        private const int N = 130000;

        private readonly IFixture _fixture = FixtureBuilder.GetInstance();
        private readonly IComparer<T> _il;
        private readonly IComparer<T> _manual;
        private readonly IComparer<T> _nito;

        private readonly T[] _one = new T[N];
        private readonly T[] _other = new T[N];

        [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private int _out;

        protected ComparersBenchmark(IComparer<T> manual, IComparer<T> il, IComparer<T> nito)
        {
            _manual = manual;
            _il = il;
            _nito = nito;
        }

        [Benchmark(Description = "IL Lighten Comparer")]
        public void IL_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _il.Compare(_one[i], _other[i]);
            }
        }

        [Benchmark(Baseline = true, Description = "Manual implementation")]
        public void Manual_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _manual.Compare(_one[i], _other[i]);
            }
        }

        [Benchmark(Description = "Nito Comparer")]
        public void Nito_Comparer()
        {
            for (var i = 0; i < N; i++) {
                _out = _nito.Compare(_one[i], _other[i]);
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            int Normalize(int value)
            {
                if (value >= 1) {
                    return 1;
                }

                if (value <= -1) {
                    return -1;
                }

                return 0;
            }

            for (var i = 0; i < N; i++) {
                _one[i] = _fixture.Create<T>();
                _other[i] = _fixture.Create<T>();

                var compare = Normalize(_manual.Compare(_one[i], _other[i]));

                if (compare != Normalize(_il.Compare(_one[i], _other[i]))) {
                    throw new InvalidOperationException("Light comparer is broken.");
                }

                if (compare != Normalize(_nito.Compare(_one[i], _other[i]))) {
                    throw new InvalidOperationException("Nito comparer is broken.");
                }
            }
        }
    }
}
