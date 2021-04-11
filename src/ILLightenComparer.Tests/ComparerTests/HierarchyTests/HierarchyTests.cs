using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests
{
    public sealed class HierarchyTests
    {
        private readonly IComparer<HierarchicalObject> _comparer;
        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        public HierarchyTests()
        {
            _comparer = new ComparerBuilder()
                        .Configure(
                            builder => builder.ConfigureFor<NestedStruct>(
                                c => c.DefineMembersOrder(
                                    order => order.Member(o => o.Property).Member(o => o.NullableProperty))))
                        .For<HierarchicalObject>()
                        .Configure(c => c.IgnoreMember(o => o.ComparableField)
                                         .IgnoreMember(o => o.Value)
                                         .IgnoreMember(o => o.FirstProperty)
                                         .IgnoreMember(o => o.SecondProperty)
                                         .IgnoreMember(o => o.NestedField)
                        )
                        .GetComparer();
        }

        [Fact]
        public void Compare_nested_null_structs()
        {
            var one = new HierarchicalObject();
            var other = new HierarchicalObject {
                NestedNullableStructProperty = _fixture.Create<NestedStruct>()
            };

            var expected = HierarchicalObject.Comparer.Compare(one, other);
            expected.Should().Be(-1);
            var actual = _comparer.Compare(one, other);

            actual.Should().Be(expected);
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

                var expected = HierarchicalObject.Comparer.Compare(one, other);
                var actual = _comparer.Compare(one, other);

                actual.Should().Be(expected);
            }
        }

        [Fact]
        public void Run_generic_tests()
        {
            var builder = new ComparerBuilder()
                          .For<SealedNestedObject>(
                              config => config.DefineMembersOrder(
                                  order => order.Member(o => o.DeepNestedField)
                                                .Member(o => o.DeepNestedProperty)
                                                .Member(o => o.Key)
                                                .Member(o => o.Text)))
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

            new GenericTests(builder).GenericTest(typeof(HierarchicalObject), HierarchicalObject.Comparer, false, Constants.BigCount);
        }
    }
}
