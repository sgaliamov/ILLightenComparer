using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
{
    public sealed class SampleCollectionMembersTests
    {
        private static void Ignoring_order_does_not_add_side_effect_for(Type sampleType, Type memberType)
        {
            var type = memberType == null
                ? sampleType
                : sampleType.MakeGenericType(memberType);

            var method = typeof(SampleCollectionMembersTests)
                         .GetGenericMethod(
                             nameof(Ignoring_order_does_not_add_side_effect_for),
                             BindingFlags.NonPublic | BindingFlags.Static)
                         .MakeGenericMethod(type);

            method.Invoke(null, null);
        }

        private static void Ignoring_order_does_not_add_side_effect_for<TElement>()
        {
            var comparer = new ComparerBuilder(c => c.SetDefaultCollectionsOrderIgnoring(true)).GetComparer<TElement[]>();

            var fixture = FixtureBuilder.GetInstance();
            var sample = fixture.Create<TElement[]>();
            var clone = sample.DeepClone();

            var elements = FixtureBuilder.GetSimpleInstance().CreateMany<TElement>().ToArray();
            comparer.Compare(sample, elements)
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
                        itemComparer = Helper.CreateNullableComparer(objectType, itemComparer);
                        objectType = objectType.MakeNullable();
                    }

                    var collectionType = useArrays
                        ? objectType.MakeArrayType()
                        : typeof(List<>).MakeGenericType(objectType);

                    var sampleType = genericSampleType.MakeGenericType(collectionType);

                    var collectionComparerType = typeof(CollectionComparer<>).MakeGenericType(objectType);
                    var collectionComparer = Activator.CreateInstance(collectionComparerType, itemComparer, sort);

                    var sampleComparerType = genericSampleComparer.MakeGenericType(collectionType);
                    var sampleComparer = (IComparer)Activator.CreateInstance(sampleComparerType, collectionComparer);

                    new GenericTests().GenericTest(sampleType, sampleComparer, sort, Constants.SmallCount);
                });
        }

        [Fact]
        public void Compare_sample_objects()
        {
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), false, false, false);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), false, false, true);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), true, false, false);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), true, false, true);
        }

        [Fact]
        public void Compare_sample_objects_ignore_order()
        {
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), false, true, false);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), false, true, true);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), true, true, false);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), true, true, true);
        }

        [Fact]
        public void Compare_sample_structs()
        {
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), false, false, false);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), false, false, true);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), true, false, false);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), true, false, true);
        }

        [Fact]
        public void Compare_sample_structs_ignore_order()
        {
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), false, true, false);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), false, true, true);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), true, true, false);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), true, true, true);
        }

        [Fact]
        public void Ignoring_order_do_not_add_side_effect()
        {
            Ignoring_order_does_not_add_side_effect_for(typeof(SampleObject<>), typeof(int));
            Ignoring_order_does_not_add_side_effect_for(typeof(SampleStruct<>), typeof(int));
            Ignoring_order_does_not_add_side_effect_for(typeof(int), null);
            Ignoring_order_does_not_add_side_effect_for(typeof(SampleObject<>), typeof(SampleObject<int>));
            Ignoring_order_does_not_add_side_effect_for(typeof(SampleStruct<>), typeof(SampleObject<int>));
        }
    }
}
