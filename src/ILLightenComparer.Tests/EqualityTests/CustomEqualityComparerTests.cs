using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests
{
    public sealed class CustomEqualityComparerTests
    {
        private static void Test(Action action) => Enumerable.Range(0, Constants.SmallCount).AsParallel().ForAll(_ => action());
        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void After_change_custom_comparer_new_dynamic_comparer_should_be_created()
        {
            Test(() => {
                var x = _fixture.Create<Tuple<int, string>>();
                var y = _fixture.Create<Tuple<int, string>>();
                var expectedEqualsInt = x.Item1.Equals(y.Item1);
                var expectedEqualsString = x.Item2?.Equals(y.Item2) ?? y.Item2 == null;
                var expectedHashInt = HashCodeCombiner.Combine(x.Item1.GetHashCode(), 0);
                var expectedHashString = HashCodeCombiner.Combine(0, x.Item2?.GetHashCode() ?? 0);

                var builder = new ComparerBuilder(c => c.SetCustomEqualityComparer(new CustomizableEqualityComparer<string>((__, _) => true, _ => 0)));
                var comparerForIntOnly = builder.GetEqualityComparer<Tuple<int, string>>();
                var comparerForStringOnly =
                    builder.Configure(c => c.SetCustomEqualityComparer<string>(null)
                                            .SetCustomEqualityComparer(new CustomizableEqualityComparer<int>((__, _) => true, _ => 0)))
                           .GetEqualityComparer<Tuple<int, string>>();

                using (new AssertionScope()) {
                    comparerForIntOnly.Equals(x, y).Should().Be(expectedEqualsInt, "comparison is based on int field");
                    comparerForStringOnly.Equals(x, y).Should().Be(expectedEqualsString, "comparison is based on string field");
                    comparerForIntOnly.GetHashCode(x).Should().Be(expectedHashInt);
                    comparerForStringOnly.GetHashCode(x).Should().Be(expectedHashString);
                }
            });
        }

        [Fact]
        public void After_clean_custom_comparer_for_value_type_dynamic_comparer_should_be_created()
        {
            var x = _fixture.Create<ComparableObject<ComparableStruct<string>>>();
            var y = _fixture.Create<ComparableObject<ComparableStruct<string>>>();

            var reference = new ComparableObjectEqualityComparer<ComparableStruct<string>>(new ComparableStructEqualityComparer<string>());
            var expectedEquals = reference.Equals(x, y);
            var expectedHash = reference.GetHashCode(x);
            var expectedCustomHash = HashCodeCombiner.Combine(0, 0);

            var builder = new ComparerBuilder(c => c.SetCustomEqualityComparer<SampleStructCustomEqualityComparer>());
            var comparerCustom = builder.GetEqualityComparer<ComparableObject<ComparableStruct<string>>>();
            var comparerDefault =
                builder.Configure(c => c.SetCustomEqualityComparer<ComparableStruct<string>>(null))
                       .GetEqualityComparer<ComparableObject<ComparableStruct<string>>>();

            using (new AssertionScope()) {
                comparerCustom.Equals(x, y).Should().BeTrue();
                comparerCustom.GetHashCode(x).Should().Be(expectedCustomHash);
                comparerDefault.Equals(x, y).Should().Be(expectedEquals);
                comparerDefault.GetHashCode(x).Should().Be(expectedHash);
            }
        }

        [Fact]
        public void Custom_comparer_defined_as_a_type_should_be_used()
        {
            Test(() => {
                var x = _fixture.Create<SampleObject<ComparableStruct<string>>>();
                var y = _fixture.Create<SampleObject<ComparableStruct<string>>>();
                var expectedCustomHash = HashCodeCombiner.Combine(0, 0);

                var comparer = new ComparerBuilder(c => c
                                                       .SetCustomEqualityComparer<SampleStructCustomEqualityComparer>())
                    .GetEqualityComparer<SampleObject<ComparableStruct<string>>>();

                using (new AssertionScope()) {
                    comparer.Equals(x, y).Should().BeTrue();
                    comparer.GetHashCode(x).Should().Be(expectedCustomHash);
                }
            });
        }

        [Fact]
        public void Custom_comparer_should_be_used_for_collection_when_defined()
        {
            Test(() => {
                var x = _fixture.Create<ComparableObject<int[]>>();
                var y = _fixture.Create<ComparableObject<int[]>>();

                var referenceComparer = new ComparableObjectEqualityComparer<int[]>(new CustomizableEqualityComparer<int[]>(
                                                                                        (a, b) => a is null && b is null || !(a is null || b is null), _ => 0));
                var expected = referenceComparer.Equals(x, y);

                var equalsIsUsed = false;
                var hashIsUsed = false;
                var comparer = new ComparerBuilder(c => c
                                                        .SetDefaultCollectionsOrderIgnoring(_fixture.Create<bool>())
                                                        .SetCustomEqualityComparer(new CustomizableEqualityComparer<int>(
                                                                                       (__, _) => {
                                                                                           equalsIsUsed = true;
                                                                                           return true;
                                                                                       },
                                                                                       _ => {
                                                                                           hashIsUsed = true;
                                                                                           return 0;
                                                                                       })))
                    .GetEqualityComparer<ComparableObject<int[]>>();

                var actualEquals = comparer.Equals(x, y);
                var hashX = comparer.GetHashCode(x);
                var hashY = comparer.GetHashCode(y);

                using (new AssertionScope()) {
                    actualEquals.Should().Be(expected);
                    hashX.Should().NotBe(0);
                    hashY.Should().NotBe(0);
                    var fieldIsNull = x.Field is null || y.Field is null;
                    var propertyIsNull = x.Property is null || y.Property is null;
                    var fieldsAreNulls = x.Field is null && y.Field is null;
                    var propertiesAreNulls = x.Property is null && y.Property is null;
                    equalsIsUsed.Should().Be(!fieldIsNull || fieldsAreNulls && !propertyIsNull, $"null checks are used.\n{x}\n{y}");
                    hashIsUsed.Should().Be(!fieldsAreNulls || !propertiesAreNulls, $"null checks are used.\n{x}\n{y}");
                }
            });
        }

        [Fact]
        public void Custom_comparer_should_be_used_for_structs()
        {
            Test(() => {
                var x = _fixture.Create<ComparableStruct<string>>();
                var y = _fixture.Create<ComparableStruct<string>>();

                var comparer = new ComparerBuilder(c => c
                                                       .SetCustomEqualityComparer<SampleStructCustomEqualityComparer>())
                    .GetEqualityComparer<ComparableStruct<string>>();

                using (new AssertionScope()) {
                    comparer.Equals(x, y).Should().BeTrue();
                    comparer.GetHashCode(x).Should().Be(0);
                }
            });
        }

        private sealed class SampleStructCustomEqualityComparer : CustomizableEqualityComparer<ComparableStruct<string>>
        {
            public SampleStructCustomEqualityComparer() : base((__, _) => true, _ => 0) { }
        }
    }
}
