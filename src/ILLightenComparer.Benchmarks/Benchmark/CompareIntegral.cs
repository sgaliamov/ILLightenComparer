using System;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace ILLightenComparer.Benchmarks.Benchmark
{
    //     Method |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD | Rank |
    // ---------- |----------:|----------:|----------:|----------:|------:|--------:|-----:|
    //        Sub |  9.300 us | 0.0563 us | 0.0527 us |  9.288 us |  1.00 |    0.00 |    1 |
    //  CompareTo |  9.329 us | 0.0517 us | 0.0484 us |  9.319 us |  1.00 |    0.01 |    1 |
    //        Ifs | 46.245 us | 0.1111 us | 0.0985 us | 46.232 us |  4.97 |    0.03 |    2 |

    [MedianColumn]
    [RankColumn]
    public class CompareIntegral
    {
        private const int N = 10000;
        private readonly byte[] _one = new byte[N];
        private readonly byte[] _other = new byte[N];

        [SuppressMessage("Code Quality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private int _out;

        [Benchmark]
        public void CompareTo()
        {
            for (var i = 0; i < N; i++) {
                _out = _one[i].CompareTo(_other[i]);
            }
        }

        [Benchmark]
        public void Ifs()
        {
            for (var i = 0; i < N; i++) {
                var one = _one[i];
                var other = _other[i];

                _out = 0;

                if (one < other) {
                    _out = -1;
                } else if (one > other) {
                    _out = 1;
                }
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random();

            for (var i = 0; i < N; i++) {
                _one[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
                _other[i] = (byte)random.Next(byte.MinValue, byte.MaxValue);
            }
        }

        [Benchmark(Baseline = true)]
        public void Sub() // fastest
        {
            for (var i = 0; i < N; i++) {
                _out = _one[i] - _other[i];
            }
        }
    }
}
