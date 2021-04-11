using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests
{
    public sealed class SampleCollectionMembersTests
    {
        private static void Ignoring_order_does_not_add_side_effect_for(Type sampleType, Type memberType)
        {
            var type = memberType == null
                ? sampleType
                : sampleType.MakeGenericType(memberType);

            var method = typeof(SampleCollectionMembersTests)
                         .GetGenericMethod(nameof(Ignoring_order_does_not_add_side_effect_for), BindingFlags.NonPublic | BindingFlags.Static)
                         .MakeGenericMethod(type);

            method.Invoke(null, null);
        }

        private static void Ignoring_order_does_not_add_side_effect_for<TElement>()
        {
            var fixture = FixtureBuilder.GetInstance();
            var sample = fixture.Create<TElement[]>();
            var clone = sample.DeepClone();

            var elements = FixtureBuilder.GetSimpleInstance().CreateMany<TElement>().ToArray();

            new ComparerBuilder(c => c.SetDefaultCollectionsOrderIgnoring(true))
                .GetComparer<TElement[]>()
                .Compare(sample, elements)
                .Should()
                .NotBe(0);

            sample.ShouldBeSameOrder(clone);
        }

        private static void Test(Type genericSampleType, Type genericSampleComparer, bool useArrays, bool sort, bool makeNullable)
        {
            Parallel.ForEach(
                TestTypes.Types,
                item => {
                    var (type, referenceComparer) = item;
                    var itemComparer = referenceComparer;
                    var objectType = type;

                    if (makeNullable && type.IsValueType) {
                        itemComparer = Helper.CreateNullableEqualityComparer(objectType, itemComparer);
                        objectType = objectType.MakeNullable();
                    }

                    var collectionType = useArrays
                        ? objectType.MakeArrayType()
                        : typeof(List<>).MakeGenericType(objectType);

                    var sampleType = genericSampleType.MakeGenericType(collectionType);
                    var collectionComparerType = typeof(CollectionEqualityComparer<>).MakeGenericType(objectType);
                    var collectionComparer = Activator.CreateInstance(collectionComparerType, itemComparer, sort, null);
                    var sampleComparerType = genericSampleComparer.MakeGenericType(collectionType);
                    var sampleComparer = (IEqualityComparer)Activator.CreateInstance(sampleComparerType, collectionComparer);

                    new GenericTests(sort).GenericTest(sampleType, sampleComparer, Constants.SmallCount);
                });
        }

        [Fact]
        public void Compare_sample_objects()
        {
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), false, false, false);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), false, false, true);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), true, false, false);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), true, false, true);
        }

        [Fact]
        public void Compare_sample_objects_ignore_order()
        {
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), false, true, false);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), false, true, true);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), true, true, false);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), true, true, true);
        }

        [Fact]
        public void Compare_sample_structs()
        {
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), false, false, false);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), false, false, true);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), true, false, false);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), true, false, true);
        }

        [Fact]
        public void Compare_sample_structs_ignore_order()
        {
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), false, true, false);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), false, true, true);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), true, true, false);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), true, true, true);
        }

        [Fact]
        public void Ignoring_order_do_not_add_side_effect()
        {
            Ignoring_order_does_not_add_side_effect_for(typeof(ComparableObject<>), typeof(int));
            Ignoring_order_does_not_add_side_effect_for(typeof(ComparableStruct<>), typeof(int));
            Ignoring_order_does_not_add_side_effect_for(typeof(int), null);
            Ignoring_order_does_not_add_side_effect_for(typeof(ComparableObject<>), typeof(ComparableObject<int>));
            Ignoring_order_does_not_add_side_effect_for(typeof(ComparableStruct<>), typeof(ComparableObject<int>));
        }
    }
}
