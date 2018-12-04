﻿using System;
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
    public class AbstractMembersTests
    {
        [Fact]
        public void AbstractProperty_Comparison()
        {
            Test(x => new AbstractMembers
            {
                AbstractProperty = x
            });
        }

        [Fact]
        public void Attempt_To_Compare_Different_Sibling_Types_Throws_ArgumentException()
        {
            var sealedNestedObject = _fixture
                                     .Build<SealedNestedObject>()
                                     .Without(x => x.DeepNestedField)
                                     .Without(x => x.DeepNestedProperty)
                                     .Create();

            var anotherNestedObject = _fixture.Create<AnotherNestedObject>();

            var one = new AbstractMembers
            {
                InterfaceField = sealedNestedObject
            };

            var another = new AbstractMembers
            {
                InterfaceField = anotherNestedObject
            };

            Assert.Throws<ArgumentException>(() => _comparer.Compare(one, another));
        }

        [Fact]
        public void Attempt_To_Compare_Inherited_Types_Throws_ArgumentException()
        {
            var sealedNestedObject = _fixture.Create<BaseNestedObject>();
            var anotherNestedObject = _fixture.Create<AnotherNestedObject>();

            var one = new AbstractMembers
            {
                NotSealedProperty = sealedNestedObject
            };

            var another = new AbstractMembers
            {
                NotSealedProperty = anotherNestedObject
            };

            Assert.Throws<ArgumentException>(() => _comparer.Compare(one, another));
        }

        [Fact]
        public void InterfaceField_Comparison()
        {
            Test(x => new AbstractMembers
            {
                InterfaceField = x
            });
        }

        [Fact]
        public void NotSealedProperty_Comparison()
        {
            Test(x => new AbstractMembers
            {
                NotSealedProperty = x
            });
        }

        [Fact]
        public void ObjectField_Comparison()
        {
            Test(x => new AbstractMembers
            {
                ObjectField = x
            });
        }

        [Fact]
        public void Replaced_Member_Does_Not_Break_Comparison()
        {
            var one = new AbstractMembers
            {
                NotSealedProperty = _fixture.Create<BaseNestedObject>()
            };

            _comparer.Compare(one, one.DeepClone()).Should().Be(0);

            var anotherNestedObject = _fixture.Create<AnotherNestedObject>();
            one.NotSealedProperty = anotherNestedObject;

            _comparer.Compare(one, one.DeepClone()).Should().Be(0);
        }

        [Fact]
        public void When_Left_Member_Is_Null_Comparison_Produces_Negative_Value()
        {
            var sealedNestedObject = _fixture.Create<BaseNestedObject>();

            var one = new AbstractMembers
            {
                NotSealedProperty = sealedNestedObject
            };

            var another = new AbstractMembers();

            _comparer.Compare(another, one).Should().BeNegative();
        }

        [Fact]
        public void When_Right_Member_Is_Null_Comparison_Produces_Positive_Value()
        {
            var sealedNestedObject = _fixture.Create<BaseNestedObject>();

            var one = new AbstractMembers
            {
                NotSealedProperty = sealedNestedObject
            };

            var another = new AbstractMembers();

            _comparer.Compare(one, another).Should().BePositive();
        }

        private void Test(Func<SealedNestedObject, AbstractMembers> selector)
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

        private readonly Fixture _fixture = FixtureBuilder.GetInstance();

        private readonly IComparer<AbstractMembers> _comparer =
            new ComparersBuilder()
                .DefineDefaultConfiguration(new ComparerSettings
                {
                    IncludeFields = true
                })
                .For<SealedNestedObject>()
                .DefineConfiguration(new ComparerSettings
                {
                    IgnoredMembers = new[]
                    {
                        nameof(SealedNestedObject.DeepNestedField),
                        nameof(SealedNestedObject.DeepNestedProperty)
                    },
                    MembersOrder = new[]
                    {
                        nameof(SealedNestedObject.Key),
                        nameof(SealedNestedObject.Text)
                    }
                })
                .For<AbstractMembers>()
                .GetComparer();
    }
}