using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
{
    public sealed class CollectionOfCollectionsTests
    {
        private static void CompareCollectionOfCollections(Func<Type, Type[]> getCollectionTypes)
        {
            Parallel.Invoke(
                () => CompareCollectionOfCollections(getCollectionTypes, null, null),
                () => CompareCollectionOfCollections(getCollectionTypes, typeof(SampleObject<>),
                                                     typeof(SampleObjectComparer<>)),
                () => CompareCollectionOfCollections(getCollectionTypes, typeof(SampleStruct<>),
                                                     typeof(SampleStructComparer<>))
            );
        }

        private static void CompareCollectionOfCollections(
            Func<Type, Type[]> getCollectionTypes,
            Type genericContainer,
            Type genericSampleComparer)
        {
            CompareCollectionOfCollections(getCollectionTypes, false, false, genericContainer, genericSampleComparer);
            CompareCollectionOfCollections(getCollectionTypes, true, false, genericContainer, genericSampleComparer);
            CompareCollectionOfCollections(getCollectionTypes, false, true, genericContainer, genericSampleComparer);
            CompareCollectionOfCollections(getCollectionTypes, true, true, genericContainer, genericSampleComparer);
        }

        private static void CompareCollectionOfCollections(
            Func<Type, Type[]> getCollectionTypes,
            bool sort,
            bool nullable,
            Type genericSampleType,
            Type genericSampleComparer)
        {
            var types = nullable ? TestTypes.NullableTypes : TestTypes.Types;
            Parallel.ForEach(types, item => {
                var (type, referenceComparer) = item;
                var collections = getCollectionTypes(type);
                var comparer = collections
                               .Prepend(type)
                               .Take(collections.Length)
                               .Select(x => typeof(CollectionComparer<>).MakeGenericType(x))
                               .Aggregate(
                                   referenceComparer,
                                   (current, comparerType) =>
                                       (IComparer)Activator.CreateInstance(comparerType, current, sort));

                type = collections.Last();

                if (genericSampleType != null) {
                    var comparerType = genericSampleComparer.MakeGenericType(type);
                    type = genericSampleType.MakeGenericType(type);
                    comparer = (IComparer)Activator.CreateInstance(comparerType, comparer);
                }

                new GenericTests().GenericTest(type, comparer, sort, 1, 2);
            });
        }

        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void Compare_array_of_array()
        {
            Type[] GetCollectionTypes(Type type)
            {
                var array1Type = type.MakeArrayType();
                var array2Type = array1Type.MakeArrayType();

                return new[] { array1Type, array2Type };
            }

            CompareCollectionOfCollections(GetCollectionTypes);
        }

        [Fact]
        public void Compare_array_of_list()
        {
            Type[] GetCollectionTypes(Type type)
            {
                var arrayType = type.MakeArrayType();
                var listType = typeof(List<>).MakeGenericType(arrayType);

                return new[] { arrayType, listType };
            }

            CompareCollectionOfCollections(GetCollectionTypes);
        }

        [Fact]
        public void Compare_enumerables_of_enumerables()
        {
            var collectionComparer = new CollectionComparer<List<int?>>(new CollectionComparer<int?>());
            var referenceComparer = new CollectionComparer<EnumerableStruct<List<int?>>?>(
                new NullableComparer<EnumerableStruct<List<int?>>>(
                    new CustomizableComparer<EnumerableStruct<List<int?>>>((a, b) =>
                                                                               collectionComparer.Compare(a, b))));
            var comparer = new ComparerBuilder().GetComparer<IEnumerable<EnumerableStruct<List<int?>>?>>();

            Helper.Parallel(() => {
                var x = _fixture.CreateMany<EnumerableStruct<List<int?>>?>().RandomNulls().ToList();
                var y = _fixture.CreateMany<EnumerableStruct<List<int?>>?>().RandomNulls().ToList();

                var expectedEquals = referenceComparer.Compare(x, y);
                var equals = comparer.Compare(x, y);

                using (new AssertionScope()) {
                    comparer.Compare(x, x).Should().Be(0);
                    equals.Should().Be(expectedEquals);
                }
            });
        }

        [Fact]
        public void Compare_list_of_array()
        {
            Type[] GetCollectionTypes(Type type)
            {
                var listType = typeof(List<>).MakeGenericType(type);
                var arrayType = listType.MakeArrayType();

                return new[] { listType, arrayType };
            }

            CompareCollectionOfCollections(GetCollectionTypes);
        }

        [Fact]
        public void Compare_list_of_list()
        {
            Type[] GetCollectionTypes(Type type)
            {
                var list1Type = typeof(List<>).MakeGenericType(type);
                var list2Type = typeof(List<>).MakeGenericType(list1Type);

                return new[] { list1Type, list2Type };
            }

            CompareCollectionOfCollections(GetCollectionTypes);
        }

        [Fact]
        public void Compare_multi_arrays()
        {
            var builder = new ComparerBuilder();

            Assert.Throws<NotSupportedException>(() => builder.For<SampleObject<IEnumerable<int[,]>>>().GetComparer());
            Assert.Throws<NotSupportedException>(() => builder.For<SampleStruct<IEnumerable<int[,]>>>().GetComparer());

            Assert.Throws<NotSupportedException>(() => builder.For<SampleObject<int[][,]>>().GetComparer());
            Assert.Throws<NotSupportedException>(() => builder.For<SampleObject<int[,]>>().GetComparer());

            Assert.Throws<NotSupportedException>(() => builder.For<SampleStruct<int[][,]>>().GetComparer());
            Assert.Throws<NotSupportedException>(() => builder.For<SampleStruct<int[,]>>().GetComparer());
        }
    }
}
