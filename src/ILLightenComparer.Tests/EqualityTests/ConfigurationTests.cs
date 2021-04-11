using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Samples;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests
{
    public sealed class ConfigurationTests
    {
        private readonly Fixture _fixture = new();

        [Fact]
        public void Ignore_all_members_using_expressions()
        {
            var x = _fixture.Create<SampleObject<int>>();
            var y = _fixture.Create<SampleObject<int>>();

            var comparer = new ComparerBuilder()
                           .For<SampleObject<int>>(c => c.IgnoreMember(o => o.Field, o => o.Property))
                           .GetEqualityComparer();

            comparer.Equals(x, y).Should().BeTrue();
        }

        [Fact]
        public void Reset_ignored_members()
        {
            var x = _fixture.Create<ComparableObject<int>>();
            var y = _fixture.Create<ComparableObject<int>>();
            var expected = new ComparableObjectEqualityComparer<int>().Equals(x, y);

            var builder = new ComparerBuilder();
            var comparer1 = builder.For<ComparableObject<int>>(c => c.IgnoreMember(o => o.Field, o => o.Property))
                                   .GetEqualityComparer();
            var comparer2 = builder.Configure(c => c.IgnoreMember<ComparableObject<int>, int>(null))
                                   .GetEqualityComparer<ComparableObject<int>>();
            var comparer3 = builder.Configure(c => c.IgnoreMember((ComparableObject<int> o) => o.Field))
                                   .Configure(c => c.IgnoreMember<ComparableObject<int>, int>(null))
                                   .GetEqualityComparer<ComparableObject<int>>();

            using (new AssertionScope()) {
                comparer1.Equals(x, y).Should().BeTrue();
                comparer2.Equals(x, y).Should().Be(expected);
                comparer3.Equals(x, y).Should().Be(expected);
            }
        }
    }
}
