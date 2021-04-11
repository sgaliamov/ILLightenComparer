using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
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
                           .GetComparer();

            comparer.Compare(x, y).Should().Be(0);
        }

        [Fact]
        public void Reset_ignored_members()
        {
            var x = _fixture.Create<SampleObject<int>>();
            var y = _fixture.Create<SampleObject<int>>();
            var expected = new SampleObjectComparer<int>().Compare(x, y);

            var builder = new ComparerBuilder();
            var comparer1 = builder.For<SampleObject<int>>(c => c.IgnoreMember(o => o.Field, o => o.Property))
                                   .GetComparer();
            var comparer2 = builder.Configure(c => c.IgnoreMember<SampleObject<int>, int>(null))
                                   .GetComparer<SampleObject<int>>();
            var comparer3 = builder.Configure(c => c.IgnoreMember((SampleObject<int> o) => o.Field))
                                   .Configure(c => c.IgnoreMember<SampleObject<int>, int>(null))
                                   .GetComparer<SampleObject<int>>();

            using (new AssertionScope()) {
                comparer1.Compare(x, y).Should().Be(0);
                comparer2.Compare(x, y).Should().Be(expected);
                comparer3.Compare(x, y).Should().Be(expected);
            }
        }
    }
}
