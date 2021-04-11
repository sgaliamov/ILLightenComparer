using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests
{
    public sealed class CollectionOfCollectionsTests
    {
        private static void CompareCollectionOfCollections(Func<Type, Type[]> getCollectionTypes)
        {
            Parallel.Invoke(
                () => CompareCollectionOfCollections(getCollectionTypes, null, null),
                () => CompareCollectionOfCollections(getCollectionTypes, typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>)),
                () => CompareCollectionOfCollections(getCollectionTypes, typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>))
            );
        }

        private static void CompareCollectionOfCollections(Func<Type, Type[]> getCollectionTypes, Type genericContainer, Type genericSampleComparer)
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
                var (itemType, itemComparer) = item;
                var collectionTypes = getCollectionTypes(itemType);
                var types = collectionTypes
                            .Prepend(itemType)
                            .Take(collectionTypes.Length);

                var sortComparersMap = types
                                       .Select(type => (type, typeof(CollectionComparer<>).MakeGenericType(type)))
                                       .Select<(Type, Type), (Type, IComparer, IComparer)>((current, prev) => {
                                           var comparer = (IComparer)Activator.CreateInstance(current.Item2, prev.Item2, sort);
                                           // build `comparer` for the current type `current.Item1` and associate the previous comparer `prev.Item2` with it.
                                           return (current.Item1, comparer, prev.Item2);
                                       })
                                       .ToDictionary(x => x.Item1, x => x.Item3);

                var refereceComparer = types
                    .Aggregate(itemComparer, (prev, type) => {
                        var comparerType = typeof(CollectionEqualityComparer<>).MakeGenericType(type);
                        return (IEqualityComparer)Activator.CreateInstance(comparerType, prev, sort, sort ? sortComparersMap[type] : null);
                    });

                var combinedType = collectionTypes.Last();

                if (genericSampleType != null) {
                    var comparerType = genericSampleComparer.MakeGenericType(combinedType);
                    combinedType = genericSampleType.MakeGenericType(combinedType);
                    refereceComparer = (IEqualityComparer)Activator.CreateInstance(comparerType, refereceComparer);
                }

                // todo: 3. try enable hash comparison
                new GenericTests(sort, false).GenericTest(combinedType, refereceComparer, 1);
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
            static int GetHashCode(IEnumerable<EnumerableStruct<List<int?>>?> input)
            {
                var num = HashCodeCombiner.Seed;
                if (input == null) {
                    return 0;
                }

                var enumerator = input.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var num2 = (num << 5) + num;
                    var enumerableStruct = enumerator.Current;
                    long num3;
                    if (enumerableStruct == null) {
                        num3 = 0L;
                    } else {
                        var num4 = HashCodeCombiner.Seed;
                        var enumerator2 = enumerableStruct.Value.GetEnumerator();
                        if (enumerator2 == null) {
                            num3 = 0L;
                        } else {
                            while (enumerator2.MoveNext()) {
                                var num5 = (num4 << 5) + num4;
                                var list = enumerator2.Current;
                                long num6;
                                if (list == null) {
                                    num6 = 0L;
                                } else {
                                    var enumerator3 = list.GetEnumerator();
                                    while (enumerator3.MoveNext()) {
                                        var num7 = (num4 << 5) + num4;
                                        var num8 = enumerator3.Current;
                                        num4 = num7 ^ (num8?.GetHashCode() ?? 0);
                                    }

                                    num6 = num4;
                                }

                                num4 = num5 ^ num6;
                            }

                            num3 = num4;
                        }
                    }

                    num = num2 ^ num3;
                }

                return (int)num;
            }

            var comparer = new ComparerBuilder().GetEqualityComparer<IEnumerable<EnumerableStruct<List<int?>>?>>();
            var listComparer = new CollectionEqualityComparer<List<int?>>(new CollectionEqualityComparer<int?>());
            var customizableComparer = new CustomizableEqualityComparer<EnumerableStruct<List<int?>>>(
                (a, b) => listComparer.Equals(a, b),
                _ => throw new NotSupportedException());
            var collectionComparer = new CollectionEqualityComparer<EnumerableStruct<List<int?>>?>(new NullableEqualityComparer<EnumerableStruct<List<int?>>>(customizableComparer));
            var referenceComparer = new CustomizableEqualityComparer<IEnumerable<EnumerableStruct<List<int?>>?>>(
                (a, b) => collectionComparer.Equals(a, b),
                GetHashCode);

            Helper.Parallel(() => {
                var x = _fixture.CreateMany<EnumerableStruct<List<int?>>?>().RandomNulls().ToList();
                var y = _fixture.CreateMany<EnumerableStruct<List<int?>>?>().RandomNulls().ToList();

                var expectedEquals = referenceComparer.Equals(x, y);
                var expectedHashX = referenceComparer.GetHashCode(x);
                var expectedHashY = referenceComparer.GetHashCode(y);

                var equals = comparer.Equals(x, y);
                var hashX = comparer.GetHashCode(x);
                var hashY = comparer.GetHashCode(y);

                using (new AssertionScope()) {
                    comparer.Equals(x, x).Should().BeTrue();
                    equals.Should().Be(expectedEquals);
                    hashX.Should().Be(expectedHashX);
                    hashY.Should().Be(expectedHashY);
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

            Assert.Throws<NotSupportedException>(() => builder.For<ComparableObject<IEnumerable<int[,]>>>().GetEqualityComparer());
            Assert.Throws<NotSupportedException>(() => builder.For<ComparableStruct<IEnumerable<int[,]>>>().GetEqualityComparer());

            Assert.Throws<NotSupportedException>(() => builder.For<ComparableObject<int[][,]>>().GetEqualityComparer());
            Assert.Throws<NotSupportedException>(() => builder.For<ComparableObject<int[,]>>().GetEqualityComparer());

            Assert.Throws<NotSupportedException>(() => builder.For<ComparableStruct<int[][,]>>().GetEqualityComparer());
            Assert.Throws<NotSupportedException>(() => builder.For<ComparableStruct<int[,]>>().GetEqualityComparer());
        }

        [Fact]
        public void Hashing_object_of_arrays_of_arrays()
        {
            var builder = new ComparerBuilder(c => c.SetDefaultCollectionsOrderIgnoring(true));

            int GetArrayHashCode(ComparableStruct<int?[][]>?[] input)
            {
                var num = 5381L;
                if (input == null) {
                    return 0;
                }

                var array = input.ToArray();
                Array.Sort(array);
                var num2 = 0;
                var length = array.Length;

                while (num2 != length) {
                    var num3 = (num << 5) + num;
                    var comparableStruct = array[num2];
                    num = num3 ^ (comparableStruct != null ? GetHashCode(comparableStruct.Value) : 0);
                    num2++;
                }

                return (int)num;
            }

            int GetHashCode(ComparableStruct<int?[][]> A_1)
            {
                var num = 5381L;
                var num2 = (num << 5) + num;
                var array = A_1.Field;
                var num3 = 0L;

                if (array != null) {
                    var comparer = builder.GetComparer<int?[]>();
                    array = array.ToArray();
                    Array.Sort(array, comparer);
                    var num4 = 0;
                    var length = array.Length;

                    while (num4 != length) {
                        var num5 = (num << 5) + num;
                        var array2 = array[num4];
                        var num6 = 0L;

                        if (array2 != null) {
                            array2 = array2.ToArray();
                            Array.Sort(array2);
                            var num7 = 0;
                            var length2 = array2.Length;

                            while (num7 != length2) {
                                var num8 = (num << 5) + num;
                                var num9 = array2[num7];
                                num = num8 ^ (num9?.GetHashCode() ?? 0);
                                num7++;
                            }

                            num6 = num;
                        }

                        num = num5 ^ num6;
                        num4++;
                    }

                    num3 = num;
                }

                num = num2 ^ num3;
                var num10 = (num << 5) + num;
                array = A_1.Property;
                var num11 = 0L;

                if (array != null) {
                    var comparer = builder.GetComparer<int?[]>();
                    array = array.ToArray();
                    Array.Sort(array, comparer);
                    var num4 = 0;
                    var length = array.Length;

                    while (num4 != length) {
                        var num12 = (num << 5) + num;
                        var array2 = array[num4];
                        var num13 = 0L;

                        if (array2 != null) {
                            array2 = array2.ToArray();
                            Array.Sort(array2);
                            var num7 = 0;
                            var length2 = array2.Length;

                            while (num7 != length2) {
                                var num14 = (num << 5) + num;
                                var num9 = array2[num7];
                                num = num14 ^ (num9?.GetHashCode() ?? 0);
                                num7++;
                            }

                            num13 = num;
                        }

                        num = num12 ^ num13;
                        num4++;
                    }

                    num11 = num;
                }

                num = num10 ^ num11;

                return (int)num;
            }

            ComparableStruct<int?[][]>.Comparer = new CollectionComparer<int?[]>(new CollectionComparer<int?>());
            var comparer = builder.GetEqualityComparer<ComparableStruct<int?[][]>?[]>();
            var referenceComparer = new CustomizableEqualityComparer<ComparableStruct<int?[][]>?[]>((__, _) => false, GetArrayHashCode);

            var x = _fixture.CreateMany<ComparableStruct<int?[][]>?>().RandomNulls().ToArray();
            var y = _fixture.CreateMany<ComparableStruct<int?[][]>?>().RandomNulls().ToArray();

            var expectedHashX = referenceComparer.GetHashCode(x);
            var expectedHashY = referenceComparer.GetHashCode(y);

            var equals = comparer.Equals(x, y);
            var hashX = comparer.GetHashCode(x);
            var hashY = comparer.GetHashCode(y);

            using (new AssertionScope()) {
                comparer.Equals(x, x).Should().BeTrue();
                hashX.Should().Be(expectedHashX);
                hashY.Should().Be(expectedHashY);
            }
        }
    }
}
