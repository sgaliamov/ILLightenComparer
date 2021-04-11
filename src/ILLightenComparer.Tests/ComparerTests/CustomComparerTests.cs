using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
{
    public sealed class CustomComparerTests
    {
        private static void Test(Action action)
        {
            Enumerable.Range(0, Constants.SmallCount).AsParallel().ForAll(_ => action());
        }

        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void After_change_custom_comparer_new_dynamic_comparer_should_be_created()
        {
            Test(() => {
                var x = _fixture.Create<Tuple<int, string>>();
                var y = _fixture.Create<Tuple<int, string>>();
                var expected1 = x.Item1.CompareTo(y.Item1);
                var expected2 = x.Item2?.CompareTo(y.Item2) ?? -y.Item2?.CompareTo(null) ?? 0;

                var builder = new ComparerBuilder(c => c.SetCustomComparer(new CustomizableComparer<string>((__, _) => 0)));
                var comparer1 = builder.GetComparer<Tuple<int, string>>();
                var comparer2 =
                    builder.Configure(c => c.SetCustomComparer<string>(null)
                                            .SetCustomComparer(new CustomizableComparer<int>((__, _) => 0)))
                           .GetComparer<Tuple<int, string>>();

                using (new AssertionScope()) {
                    comparer1.Compare(x, y).Normalize().Should().Be(expected1.Normalize());
                    comparer2.Compare(x, y).Normalize().Should().Be(expected2.Normalize());
                }
            });
        }

        [Fact]
        public void After_clean_custom_comparer_for_value_type_dynamic_comparer_should_be_created()
        {
            var x = _fixture.Create<SampleObject<SampleStruct<string>>>();
            var y = _fixture.Create<SampleObject<SampleStruct<string>>>();

            var reference = new SampleObjectComparer<SampleStruct<string>>(new SampleStructComparer<string>());
            var expected = reference.Compare(x, y);

            var builder = new ComparerBuilder(c => c.SetCustomComparer<SampleStructCustomComparer>());
            var comparer1 = builder.GetComparer<SampleObject<SampleStruct<string>>>();
            var comparer2 = builder.Configure(c => c.SetCustomComparer<SampleStruct<string>>(null))
                                   .GetComparer<SampleObject<SampleStruct<string>>>();

            using (new AssertionScope()) {
                comparer1.Compare(x, y).Should().Be(0);
                comparer2.Compare(x, y).Normalize().Should().Be(expected.Normalize());
            }
        }

        [Fact]
        public void Custom_comparer_defined_as_a_type_should_be_used()
        {
            Test(() => {
                var x = _fixture.Create<SampleObject<SampleStruct<string>>>();
                var y = _fixture.Create<SampleObject<SampleStruct<string>>>();

                var comparer =
                    new ComparerBuilder(c => c.SetCustomComparer<SampleStructCustomComparer>())
                        .GetComparer<SampleObject<SampleStruct<string>>>();

                comparer.Compare(x, y).Should().Be(0);
            });
        }

        [Fact]
        public void Custom_comparer_should_be_used_for_collection_when_defined()
        {
            Test(() => {
                var x = _fixture.Create<SampleObject<int[]>>();
                var y = _fixture.Create<SampleObject<int[]>>();

                var referenceComparer = new SampleObjectComparer<int[]>(new CustomizableComparer<int[]>((a, b) => {
                    if (a == b) {
                        return 0;
                    }

                    if (b is null) {
                        return 1;
                    }

                    if (a is null) {
                        return -1;
                    }

                    return 0;
                }));
                var expected = referenceComparer.Compare(x, y);

                var comparer =
                    new ComparerBuilder(c => c.SetDefaultCollectionsOrderIgnoring(_fixture.Create<bool>())
                                              .SetCustomComparer(new CustomizableComparer<int>((__, _) => 0)))
                        .GetComparer<SampleObject<int[]>>();

                var actual = comparer.Compare(x, y);

                actual.Should().Be(expected);
            });
        }

        [Fact]
        public void Custom_comparer_should_be_used_for_itself()
        {
            Test(() => {
                var x = _fixture.Create<SampleStruct<string>>();
                var y = _fixture.Create<SampleStruct<string>>();

                var comparer =
                    new ComparerBuilder(c => c.SetCustomComparer<SampleStructCustomComparer>())
                        .GetComparer<SampleStruct<string>>();

                comparer.Compare(x, y).Should().Be(0);
            });
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class SampleStructCustomComparer : CustomizableComparer<SampleStruct<string>>
        {
            public SampleStructCustomComparer() : base((_, __) => 0) { }
        }
    }
}
