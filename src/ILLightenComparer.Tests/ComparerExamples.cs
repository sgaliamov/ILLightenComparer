using System;
using AutoFixture;
using FluentAssertions;
using ILLightenComparer.Tests.Comparers;
using Xunit;

namespace ILLightenComparer.Tests
{
    public sealed class ComparerExamples
    {
        private readonly Fixture _fixture = new();

        [Fact]
        public void Basic_usage()
        {
            var x = _fixture.Create<Tuple<int, string>>();
            var y = _fixture.Create<Tuple<int, string>>();

            var comparer = new ComparerBuilder().GetComparer<Tuple<int, string>>();

            var result = comparer.Compare(x, y);

            result.Should().NotBe(0);
        }

        [Fact]
        public void Define_custom_comparer()
        {
            var x = _fixture.Create<Tuple<int, string>>();
            var y = _fixture.Create<Tuple<int, string>>();
            var customComparer = new CustomizableComparer<Tuple<int, string>>((a, b) => 0); // makes all objects always equal

            var comparer = new ComparerBuilder()
                           .Configure(c => c.SetCustomComparer(customComparer))
                           .GetComparer<Tuple<int, string>>();

            var result = comparer.Compare(x, y);

            result.Should().Be(0);
        }

        [Fact]
        public void Define_multiple_configurations()
        {
            var builder = new ComparerBuilder(c => c.SetDefaultCyclesDetection(false)); // defines initial configuration

            // adds some configuration later
            builder.Configure(c => c.SetStringComparisonType(
                                        typeof(Tuple<int, string, Tuple<short, string>>),
                                        StringComparison.InvariantCultureIgnoreCase)
                                    .IgnoreMember<Tuple<int, string, Tuple<short, string>>, int>(o => o.Item1));

            // defines configuration for specific types
            builder.For<Tuple<short, string>>(c => c.DefineMembersOrder(
                                                  order => order.Member(o => o.Item2)
                                                                .Member(o => o.Item2)));

            // adds additional configuration to existing configuration
            builder.For<Tuple<int, string, Tuple<short, string>>>(c => c.IncludeFields(false));

            var comparer = builder.GetComparer<Tuple<int, string, Tuple<short, string>>>();

            comparer.Should().NotBeNull();
        }

        [Fact]
        public void Fixed_configuration()
        {
            var x = new Tuple<int, string>(1, "text");
            var y = new Tuple<int, string>(2, "TEXT");

            // initially configuration defines case insensitive string comparison
            var builder = new ComparerBuilder()
                .For<Tuple<int, string>>(c => c.SetStringComparisonType(StringComparison.CurrentCultureIgnoreCase)
                                               .DetectCycles(false));

            // in addition, setup to ignore first member
            builder.Configure(c => c.IgnoreMember(o => o.Item1));

            // this version takes in account only case insensitive second member
            var ignoreCaseComparer = builder.GetComparer();

            // override string comparison type with case sensitive value and build new comparer
            var originalCaseComparer = builder
                                       .For<Tuple<int, string>>()
                                       .Configure(c => c.SetStringComparisonType(StringComparison.Ordinal))
                                       .GetComparer();

            // first comparer ignores case for strings still
            ignoreCaseComparer.Compare(x, y).Should().Be(0);

            // second comparer ignores first member but uses new string comparison type still
            var result = originalCaseComparer.Compare(x, y);

            result.Should().Be(string.CompareOrdinal("text", "TEXT"));
        }

        [Fact]
        public void Ignore_collection_order()
        {
            var x = new[] { 1, 2, 3 };
            var y = new[] { 2, 3, 1 };

            var comparer = new ComparerBuilder()
                           .For<int[]>(c => c.IgnoreCollectionsOrder(true))
                           .GetComparer();

            var result = comparer.Compare(x, y);

            result.Should().Be(0);
        }

        [Fact]
        public void Ignore_specific_members()
        {
            var x = new Tuple<int, string, double>(1, "value 1", 1.1);
            var y = new Tuple<int, string, double>(1, "value 2", 2.2);

            var comparer = new ComparerBuilder()
                           .For<Tuple<int, string, double>>()
                           .Configure(c => c.IgnoreMember(o => o.Item2)
                                            .IgnoreMember(o => o.Item3))
                           .GetComparer();

            var result = comparer.Compare(x, y);

            result.Should().Be(0);
        }
    }
}
