using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Force.DeepCloner;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests
{
    public sealed class AbstractMembersTests
    {
        private readonly IEqualityComparer<AbstractMembers> _comparer =
            new ComparerBuilder()
                .For<AnotherNestedObject>(c => c.DefineMembersOrder(
                                              order => order.Member(o => o.Value)
                                                            .Member(o => o.Key)
                                                            .Member(o => o.Text)))
                .For<AbstractMembers>()
                .GetEqualityComparer();

        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void Abstract_property_comparison() => TestOneMember(x => new AbstractMembers { AbstractProperty = x }, Constants.SmallCount);

        [Fact]
        public void Different_Inherited_types_are_not_equal()
        {
            var sealedNestedObject = _fixture.Create<BaseNestedObject>();
            var anotherNestedObject = _fixture.Create<AnotherNestedObject>();

            var one = new AbstractMembers {
                NotSealedProperty = sealedNestedObject
            };

            var another = new AbstractMembers {
                NotSealedProperty = anotherNestedObject
            };

            _comparer.Equals(one, another).Should().BeFalse();
        }

        [Fact]
        public void Different_sibling_types_are_not_equals()
        {
            var sealedNestedObject = _fixture
                                     .Build<SealedNestedObject>()
                                     .Without(x => x.DeepNestedField)
                                     .Without(x => x.DeepNestedProperty)
                                     .Create();

            var anotherNestedObject = _fixture.Create<AnotherNestedObject>();

            var one = new AbstractMembers {
                InterfaceField = sealedNestedObject
            };

            var another = new AbstractMembers {
                InterfaceField = anotherNestedObject
            };

            _comparer.Equals(one, another).Should().BeFalse();
        }

        [Fact]
        public void Interface_field_comparison() => TestOneMember(x => new AbstractMembers { InterfaceField = x }, Constants.SmallCount);

        [Fact]
        public void Not_sealed_property_comparison() => TestOneMember(x => new AbstractMembers { NotSealedProperty = x }, Constants.SmallCount);

        [Fact]
        public void Object_field_comparison() => TestOneMember(x => new AbstractMembers { ObjectField = x }, Constants.BigCount);

        [Fact]
        public void Replaced_member_does_not_break_comparison()
        {
            var one = new AbstractMembers {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };
            _comparer.Equals(one, one.DeepClone()).Should().BeTrue();

            for (var i = 0; i < 100; i++) {
                one.NotSealedProperty = _fixture.Create<AnotherNestedObject>();
                var other = new AbstractMembers {
                    NotSealedProperty = _fixture.Create<AnotherNestedObject>()
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

        private void TestOneMember(Func<SealedNestedObject, AbstractMembers> selector, int count)
        {
            var x = _fixture
                    .Build<SealedNestedObject>()
                    .Without(x => x.DeepNestedField)
                    .Without(x => x.DeepNestedProperty)
                    .CreateMany(count)
                    .Select(selector)
                    .ToArray();

            var y = _fixture
                    .Build<SealedNestedObject>()
                    .Without(x => x.DeepNestedField)
                    .Without(x => x.DeepNestedProperty)
                    .CreateMany(count)
                    .Select(selector)
                    .ToArray();

            for (var i = 0; i < count; i++) {
                var expectedHashX = x[i].GetHashCode();
                var expectedHashY = y[i].GetHashCode();
                var expectedEquals = x[i].Equals(y[i]);

                var hashX = _comparer.GetHashCode(x[i]);
                var hashY = _comparer.GetHashCode(y[i]);
                var equals = _comparer.Equals(x[i], y[i]);

                using (new AssertionScope()) {
                    equals.Should().Be(expectedEquals);
                    hashX.Should().Be(expectedHashX);
                    hashY.Should().Be(expectedHashY);
                }
            }
        }

        [Fact]
        public void When_left_member_is_null_comparison_produces_false()
        {
            var one = new AbstractMembers();
            var other = new AbstractMembers {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };

            _comparer.Equals(one, other).Should().BeFalse();
        }

        [Fact]
        public void When_right_member_is_null_comparison_produces_false()
        {
            var one = new AbstractMembers {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };
            var another = new AbstractMembers();

            _comparer.Equals(one, another).Should().BeFalse();
        }
    }
}
