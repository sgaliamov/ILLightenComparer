using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests
{
    public sealed class SampleTypesTests
    {
        private static readonly IFixture _fixture = FixtureBuilder.GetInstance();

        private static void TestCollection(Type genericCollectionType = null) =>
            Parallel.ForEach(TestTypes.Types, item => {
                var (type, referenceComparer) = item;
                TestCollection(type, referenceComparer, genericCollectionType, false);
                TestCollection(type, referenceComparer, genericCollectionType, true);
            });

        private static void TestNullableCollection(Type genericCollectionType = null)
        {
            foreach (var (nullableType, nullableComparer) in TestTypes.NullableTypes) {
                TestCollection(nullableType, nullableComparer, genericCollectionType, false);
                TestCollection(nullableType, nullableComparer, genericCollectionType, true);
            }
        }

        private static void TestCollection(Type objectType, IEqualityComparer itemComparer, Type genericCollectionType, bool sort)
        {
            var collectionType = genericCollectionType == null
                ? objectType.MakeArrayType()
                : genericCollectionType.MakeGenericType(objectType);

            var comparerType = typeof(CollectionEqualityComparer<>).MakeGenericType(objectType);
            var constructor = comparerType.GetConstructor(new[] { typeof(IEqualityComparer<>).MakeGenericType(objectType), typeof(bool), typeof(IComparer<>).MakeGenericType(objectType) });
            var comparer = (IEqualityComparer)constructor.Invoke(new object[] { itemComparer, sort, null });

            new GenericTests(sort).GenericTest(collectionType, comparer, Constants.SmallCount);
        }

        [Fact]
        public void Compare_arrays_directly() => TestCollection();

        [Fact]
        public void Compare_arrays_of_nullables_directly() => TestNullableCollection();

        [Fact]
        public void Compare_enumerables_directly() => TestCollection(typeof(IEnumerable<>));

        [Fact]
        public void Compare_enumerables_of_nullables_directly() => TestNullableCollection(typeof(IEnumerable<>));

        [Fact]
        public void Compare_nullable_types_directly()
        {
            foreach (var (nullableType, nullableComparer) in TestTypes.NullableTypes) {
                new GenericTests(false).GenericTest(nullableType, nullableComparer, Constants.SmallCount);
            }
        }

        [Fact]
        public void Compare_types_directly() =>
            Parallel.ForEach(TestTypes.Types, item => {
                var (type, referenceComparer) = item;
                new GenericTests(false).GenericTest(type, referenceComparer, Constants.SmallCount);
            });

        [Fact]
        public void Should_use_delayed_comparison()
        {
            var x = _fixture.CreateMany<SampleEqualityStruct<EnumSmall?>?>().ToArray();
            var y = _fixture.CreateMany<SampleEqualityStruct<EnumSmall?>?>().ToArray();

            var referenceComparer = new CollectionEqualityComparer<SampleEqualityStruct<EnumSmall?>?>(
                new NullableEqualityComparer<SampleEqualityStruct<EnumSmall?>>());

            var comparer = new ComparerBuilder().GetEqualityComparer<object>();

            var expectedHashX = referenceComparer.GetHashCode(x);
            var expectedHashY = referenceComparer.GetHashCode(y);
            var actualHashX = comparer.GetHashCode(x);
            var actualHashY = comparer.GetHashCode(y);
            var expected = referenceComparer.Equals(x, y);
            var actual = comparer.Equals(x, y);

            using (new AssertionScope()) {
                actualHashX.Should().Be(expectedHashX);
                actualHashY.Should().Be(expectedHashY);
                actual.Should().Be(expected);
            }
        }
    }
}
