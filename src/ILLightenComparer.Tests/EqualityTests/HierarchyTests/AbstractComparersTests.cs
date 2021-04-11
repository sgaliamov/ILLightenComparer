using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests
{
    public sealed class AbstractComparersTests
    {
        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void Comparison_uses_only_members_of_abstract_class()
        {
            var builder = new ComparerBuilder().For<AbstractNestedObject>(c => c.DetectCycles(false));

            Test(builder);
        }

        [Fact]
        public void Comparison_uses_only_members_of_base_class()
        {
            var builder = new ComparerBuilder().For<BaseNestedObject>(
                c => c.DetectCycles(false).IgnoreMember(o => o.Key));

            Test(builder);
        }

        [Fact]
        public void Comparison_uses_only_members_of_base_interface()
        {
            var builder = new ComparerBuilder().For<INestedObject>(c => c.DetectCycles(false));

            Test(builder);
        }

        private void Test<T>(IEqualityComparerProvider<T> builder)
            where T : INestedObject
        {
            var comparer = builder.GetEqualityComparer();
            var referenceComparer = new CustomizableEqualityComparer<INestedObject>(
                (a, b) => a.Text?.Equals(b.Text) ?? b.Text is null,
                x => HashCodeCombiner.Combine(x.Text));

            for (var i = 0; i < 10; i++) {
                var x = (T)(object)_fixture.Create<SealedNestedObject>();
                var y = (T)(object)_fixture.Create<SealedNestedObject>();

                var expectedHashX = referenceComparer.GetHashCode(x);
                var expectedHashY = referenceComparer.GetHashCode(y);
                var expectedEquals = referenceComparer.Equals(x, y);

                var hashX = comparer.GetHashCode(x);
                var hashY = comparer.GetHashCode(y);
                var equals = comparer.Equals(x, y);

                using (new AssertionScope()) {
                    equals.Should().Be(expectedEquals);
                    hashX.Should().Be(expectedHashX);
                    hashY.Should().Be(expectedHashY);
                }
            }
        }
    }
}
