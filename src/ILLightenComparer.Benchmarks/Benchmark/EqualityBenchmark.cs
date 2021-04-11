using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Illuminator;
using static Illuminator.Functions;

namespace ILLightenComparer.Benchmarks.Benchmark
{
    [MedianColumn]
    [RankColumn]
    public class EqualityBenchmark
    {
        private const int N = 10000;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Sub(int a, int b) => a - b == 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Equals(int a, int b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Operator(int a, int b) => a == b;

        private readonly int[] _one = new int[N];
        private readonly int[] _other = new int[N];

        // ReSharper disable once NotAccessedField.Local
        private bool _out;

        private Func<int, int, bool> _subCompare;
        private Func<int, int, bool> _subNot;

        [Benchmark]
        public void Equals()
        {
            for (var i = 0; i < N; i++) {
                _out = Equals(_one[i], _other[i]);
            }
        }

        [Benchmark]
        public void GeneratedSub()
        {
            for (var i = 0; i < N; i++) {
                _out = _subCompare(_one[i], _other[i]);
            }
        }

        [Benchmark]
        public void GeneratedSubNot()
        {
            for (var i = 0; i < N; i++) {
                _out = _subNot(_one[i], _other[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void Operator()
        {
            for (var i = 0; i < N; i++) {
                _out = Operator(_one[i], _other[i]);
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random();

            for (var i = 0; i < N; i++) {
                _one[i] = random.Next(int.MinValue, int.MaxValue);
                _other[i] = random.Next(int.MinValue, int.MaxValue);
            }

            var subCompare = new DynamicMethod("SubCompare", typeof(bool), new[] { typeof(int), typeof(int) });
            using (var il = subCompare.GetILGenerator().UseIlluminator()) {
                il.Sub(Ldarg(0), Ldarg(1))
                  .Brfalse_S(out var equals)
                  .Ldc_I4_0()
                  .Ret()
                  .MarkLabel(equals)
                  .Ldc_I4_1()
                  .Ret();

                _subCompare = subCompare.CreateDelegate<Func<int, int, bool>>();
            }

            var subNot = new DynamicMethod("SubNot", typeof(bool), new[] { typeof(int), typeof(int) });
            using (var il = subNot.GetILGenerator().UseIlluminator()) {
                il.Sub(Ldarg(0), Ldarg(1))
                  .Not()
                  .Ret();

                _subNot = subNot.CreateDelegate<Func<int, int, bool>>();
            }
        }

        [Benchmark]
        public void Sub()
        {
            for (var i = 0; i < N; i++) {
                _out = Sub(_one[i], _other[i]);
            }
        }
    }
}
