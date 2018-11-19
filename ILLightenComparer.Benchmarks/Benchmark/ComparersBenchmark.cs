﻿using System;
using System.Collections.Generic;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Nito.Comparers;

namespace ILLightenComparer.Benchmarks.Benchmark
{
//           Method |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD | Rank |
// ---------------- |----------:|----------:|----------:|----------:|------:|--------:|-----:|
//    Nito_Comparer | 12.456 us | 0.2484 us | 0.5855 us | 12.356 us |  7.89 |    0.43 |    3 |
//      IL_Comparer |  1.851 us | 0.0483 us | 0.0708 us |  1.840 us |  1.15 |    0.07 |    2 |
//  Native_Comparer |  1.597 us | 0.0404 us | 0.0779 us |  1.571 us |  1.00 |    0.00 |    1 |

    [MedianColumn]
    [RankColumn]
    public class ComparersBenchmark
    {
        private const int N = 100;

        private static readonly IComparer<TestObject> Native = TestObject.TestObjectComparer;

        private static readonly IComparer<TestObject> ILLightenComparer =
            new ComparersBuilder().CreateComparer<TestObject>();

        private static readonly IComparer<TestObject> NitoComparer = ComparerBuilder
                                                                     .For<TestObject>()
                                                                     .OrderBy(x => x.BooleanProperty)
                                                                     .ThenBy(x => x.ByteProperty)
                                                                     .ThenBy(x => x.SByteProperty)
                                                                     .ThenBy(x => x.CharProperty)
                                                                     .ThenBy(x => x.DecimalProperty)
                                                                     .ThenBy(x => x.DoubleProperty)
                                                                     .ThenBy(x => x.SingleProperty)
                                                                     .ThenBy(x => x.Int32Property)
                                                                     .ThenBy(x => x.UInt32Property)
                                                                     .ThenBy(x => x.Int64Property)
                                                                     .ThenBy(x => x.UInt64Property)
                                                                     .ThenBy(x => x.Int16Property)
                                                                     .ThenBy(x => x.UInt16Property)
                                                                     .ThenBy(x => x.StringProperty);

        private static readonly Fixture Fixture = FixtureBuilder.GetInstance();

        private readonly TestObject[] _one = new TestObject[N];
        private readonly TestObject[] _other = new TestObject[N];

        // ReSharper disable once NotAccessedField.Local
        private int _out;

        [GlobalSetup]
        public void Setup()
        {
            for (var i = 0; i < N; i++)
            {
                _one[i] = Fixture.Create<TestObject>();
                _other[i] = Fixture.Create<TestObject>();

                var compare = Native.Compare(_one[i], _other[i]);
                if (compare != NitoComparer.Compare(_one[i], _other[i])
                    || compare != ILLightenComparer.Compare(_one[i], _other[i]))
                {
                    throw new InvalidOperationException("Some comparer is broken.");
                }
            }
        }

        [Benchmark]
        public void Nito_Comparer()
        {
            for (var i = 0; i < N; i++)
            {
                _out = NitoComparer.Compare(_one[i], _other[i]);
            }
        }

        [Benchmark]
        public void IL_Comparer() // fastest
        {
            for (var i = 0; i < N; i++)
            {
                _out = ILLightenComparer.Compare(_one[i], _other[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void Native_Comparer()
        {
            for (var i = 0; i < N; i++)
            {
                _out = Native.Compare(_one[i], _other[i]);
            }
        }
    }
}
