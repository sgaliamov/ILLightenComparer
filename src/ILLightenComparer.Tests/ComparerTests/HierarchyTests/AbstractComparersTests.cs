using AutoFixture;
using FluentAssertions;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests
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

        private void Test<T>(IComparerProvider<T> builder)
            where T : INestedObject
        {
            var comparer = builder.GetComparer();

            for (var i = 0; i < 10; i++) {
                var x = (T)(object)_fixture.Create<SealedNestedObject>();
                var y = (T)(object)_fixture.Create<SealedNestedObject>();

                var expected = string.CompareOrdinal(x.Text, y.Text).Normalize();
                var actual = comparer.Compare(x, y).Normalize();

                actual.Should().Be(expected, $"\nx: {x.ToJson()},\ny: {y.ToJson()}");
            }
        }
    }
}
