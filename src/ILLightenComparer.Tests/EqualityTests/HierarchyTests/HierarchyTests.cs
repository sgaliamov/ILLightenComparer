using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests
{
    public sealed class HierarchyTests
    {
        private readonly IComparerBuilder _builder;
        private readonly IEqualityComparer<HierarchicalObject> _comparer;
        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        public HierarchyTests()
        {
            _builder = new ComparerBuilder()
                       .For<NestedStruct>(
                           config => config.DefineMembersOrder(
                               order => order.Member(o => o.Property)
                                             .Member(o => o.NullableProperty)))
                       .For<HierarchicalObject>(
                           config => config.DefineMembersOrder(
                               order => order.Member(o => o.ComparableField)
                                             .Member(o => o.Value)
                                             .Member(o => o.FirstProperty)
                                             .Member(o => o.SecondProperty)
                                             .Member(o => o.NestedField)
                                             .Member(o => o.NestedStructField)
                                             .Member(o => o.NestedNullableStructField)
                                             .Member(o => o.NestedStructProperty)
                                             .Member(o => o.NestedNullableStructProperty)))
                       .Builder;

            _comparer = _builder.GetEqualityComparer<HierarchicalObject>();
        }

        [Fact]
        public void Compare_nested_null_structs()
        {
            var one = new HierarchicalObject();
            var other = new HierarchicalObject {
                NestedNullableStructProperty = _fixture.Create<NestedStruct>()
            };

            var expectedHashX = one.GetHashCode();
            var expectedHashY = other.GetHashCode();
            var expectedEquals = one.Equals(other);

            var hashX = _comparer.GetHashCode(one);
            var hashY = _comparer.GetHashCode(other);
            var equals = _comparer.Equals(one, other);

            using (new AssertionScope()) {
                equals.Should().Be(expectedEquals);
                hashX.Should().Be(expectedHashX);
                hashY.Should().Be(expectedHashY);
            }
        }

        [Fact]
        public void Compare_nested_structs()
        {
            for (var i = 0; i < 10; i++) {
                var one = new HierarchicalObject {
                    NestedStructField = _fixture.Create<NestedStruct>(),
                    NestedNullableStructProperty = _fixture.Create<NestedStruct>()
                };

                var other = new HierarchicalObject {
                    NestedStructField = _fixture.Create<NestedStruct>(),
                    NestedNullableStructProperty = _fixture.Create<NestedStruct>()
                };

                var expectedHashX = one.GetHashCode();
                var expectedHashY = other.GetHashCode();
                var expectedEquals = one.Equals(other);

                var hashX = _comparer.GetHashCode(one);
                var hashY = _comparer.GetHashCode(other);
                var equals = _comparer.Equals(one, other);

                using (new AssertionScope()) {
                    equals.Should().Be(expectedEquals);
                    hashX.Should().Be(expectedHashX);
                    hashY.Should().Be(expectedHashY);
                }
            }
        }

        [Fact]
        public void Run_generic_tests() => new GenericTests(false, comparerBuilder: _builder)
            .GenericTest(typeof(HierarchicalObject), null, Constants.BigCount);
    }
}
