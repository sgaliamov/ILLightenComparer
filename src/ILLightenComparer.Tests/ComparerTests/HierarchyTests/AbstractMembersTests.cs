using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests
{
    public sealed class AbstractMembersTests
    {
        private readonly IComparer<AbstractMembers> _comparer =
            new ComparerBuilder()
                .For<SealedNestedObject>(c =>
                                             c.IgnoreMember(o => o.DeepNestedField)
                                              .IgnoreMember(o => o.DeepNestedProperty)
                                              .DefineMembersOrder(order =>
                                                                      order.Member(o => o.Key)
                                                                           .Member(o => o.Text)))
                .For<AnotherNestedObject>(c => c
                                              .DefineMembersOrder(order =>
                                                                      order.Member(o => o.Value)
                                                                           .Member(o => o.Key)
                                                                           .Member(o => o.Text))
                )
                .For<AbstractMembers>()
                .GetComparer();

        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void Abstract_property_comparison()
        {
            TestOneField(x => new AbstractMembers { AbstractProperty = x });
        }

        [Fact]
        public void Attempt_to_compare_different_sibling_types_throws_argument_exception()
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

            Assert.Throws<ArgumentException>(() => _comparer.Compare(one, another));
        }

        [Fact]
        public void Attempt_to_compare_inherited_types_throws_argument_exception()
        {
            var sealedNestedObject = _fixture.Create<BaseNestedObject>();
            var anotherNestedObject = _fixture.Create<AnotherNestedObject>();

            var one = new AbstractMembers {
                NotSealedProperty = sealedNestedObject
            };

            var another = new AbstractMembers {
                NotSealedProperty = anotherNestedObject
            };

            Assert.Throws<ArgumentException>(() => _comparer.Compare(one, another));
        }

        [Fact]
        public void Interface_field_comparison()
        {
            TestOneField(x => new AbstractMembers { InterfaceField = x });
        }

        [Fact]
        public void Not_sealed_property_comparison()
        {
            TestOneField(x => new AbstractMembers { NotSealedProperty = x });
        }

        [Fact]
        public void Object_field_comparison()
        {
            TestOneField(x => new AbstractMembers { ObjectField = x });
        }

        [Fact]
        public void Replaced_member_does_not_break_comparison()
        {
            var one = new AbstractMembers {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };
            _comparer.Compare(one, one.DeepClone()).Should().Be(0);

            for (var i = 0; i < 100; i++) {
                one.NotSealedProperty = _fixture.Create<AnotherNestedObject>();
                var other = new AbstractMembers {
                    NotSealedProperty = _fixture.Create<AnotherNestedObject>()
                };

                var expected = AbstractMembers.Comparer.Compare(one, other).Normalize();
                var actual = _comparer.Compare(one, other).Normalize();

                actual.Should().Be(expected);
            }
        }

        private void TestOneField(Func<SealedNestedObject, AbstractMembers> selector)
        {
            var original = _fixture
                           .Build<SealedNestedObject>()
                           .Without(x => x.DeepNestedField)
                           .Without(x => x.DeepNestedProperty)
                           .CreateMany(1000)
                           .Select(selector)
                           .ToArray();

            var clone = original.DeepClone();

            Array.Sort(original, AbstractMembers.Comparer);
            Array.Sort(clone, _comparer);

            original.ShouldBeSameOrder(clone);
        }

        [Fact]
        public void When_left_member_is_null_comparison_produces_negative_value()
        {
            var one = new AbstractMembers();
            var other = new AbstractMembers {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };

            _comparer.Compare(one, other).Should().BeNegative();
        }

        [Fact]
        public void When_right_member_is_null_comparison_produces_positive_value()
        {
            var one = new AbstractMembers {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };
            var another = new AbstractMembers();

            _comparer.Compare(one, another).Should().BePositive();
        }
    }
}
