using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
{
    public sealed class SampleComparableTests
    {
        private static void Test(Type comparableGenericType, bool makeNullable)
        {
            var types = makeNullable ? TestTypes.NullableTypes : TestTypes.Types;

            Parallel.ForEach(types, item => {
                var (objectType, referenceComparer) = item;
                var itemComparer = referenceComparer;
                var comparableType = comparableGenericType.MakeGenericType(objectType);

                if (itemComparer != null) {
                    typeof(ComparableStruct<>)
                        .MakeGenericType(objectType)
                        .GetField(nameof(ComparableStruct<object>.Comparer), BindingFlags.Public | BindingFlags.Static)
                        .SetValue(null, itemComparer);

                    typeof(ComparableBaseObject<>)
                        .MakeGenericType(objectType)
                        .GetField(nameof(ComparableBaseObject<object>.Comparer), BindingFlags.Public | BindingFlags.Static)
                        .SetValue(null, itemComparer);

                    typeof(ComparableChildObject<>)
                        .MakeGenericType(objectType)
                        .GetField(nameof(ComparableChildObject<object>.ChildComparer), BindingFlags.Public | BindingFlags.Static)
                        .SetValue(null, itemComparer);
                }

                comparableType
                    .GetField("UsedCompareTo", BindingFlags.Public | BindingFlags.Static)?
                    .SetValue(null, false);

                new GenericTests().GenericTest(comparableType, null, false, Constants.SmallCount);

                if (comparableType.IsSealedType()) {
                    comparableType
                        .GetField("UsedCompareTo", BindingFlags.Public | BindingFlags.Static)
                        .GetValue(null)
                        .Should()
                        .Be(true, comparableType.FullName);
                }
            });
        }

        [Fact]
        public void Compare_comparable_base_objects()
        {
            Test(typeof(ComparableBaseObject<>), false);
            Test(typeof(ComparableBaseObject<>), true);
        }

        [Fact]
        public void Compare_comparable_child_objects()
        {
            Test(typeof(ComparableChildObject<>), false);
            Test(typeof(ComparableChildObject<>), true);
        }

        [Fact]
        public void Compare_comparable_structs()
        {
            Test(typeof(ComparableStruct<>), false);
            Test(typeof(ComparableStruct<>), true);
        }

        [Fact]
        public void Custom_comparable_implementation_should_return_negative_when_first_argument_isnull()
        {
            var one = new SampleObject<ComparableBaseObject<EnumSmall>> {
                Property = FixtureBuilder.GetInstance().Create<ComparableBaseObject<EnumSmall>>()
            };

            var other = one.DeepClone();
            one.Property = null;

            var comparer = new ComparerBuilder().GetComparer<SampleObject<ComparableBaseObject<EnumSmall>>>();

            comparer.Compare(one, other).Should().BeNegative();
        }

        [Fact]
        public void Replaced_comparable_object_is_compared_with_custom_implementation()
        {
            var comparer = new ComparerBuilder().GetComparer<SampleObject<ComparableBaseObject<EnumSmall>>>();
            var fixture = FixtureBuilder.GetInstance();

            var one = new SampleObject<ComparableBaseObject<EnumSmall>> {
                Property = fixture.Create<ComparableChildObject<EnumSmall>>()
            };
            comparer.Compare(one, one.DeepClone()).Should().Be(0);

            for (var i = 0; i < Constants.SmallCount; i++) {
                one.Property = fixture.Create<ComparableChildObject<EnumSmall>>();
                var other = new SampleObject<ComparableBaseObject<EnumSmall>> {
                    Property = fixture.Create<ComparableChildObject<EnumSmall>>()
                };

                var expected = one.Property.CompareTo(other.Property).Normalize();
                var actual = comparer.Compare(one, other).Normalize();

                actual.Should().Be(expected);
            }

            ComparableChildObject<EnumSmall>.UsedCompareTo.Should().BeTrue();
        }
    }
}
