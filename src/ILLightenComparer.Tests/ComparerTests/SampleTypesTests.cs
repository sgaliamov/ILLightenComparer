using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
{
    public sealed class SampleTypesTests
    {
        private static readonly IFixture Fixture = FixtureBuilder.GetInstance();

        private static void TestCollection(Type genericCollectionType = null)
        {
            Parallel.ForEach(TestTypes.Types, item => {
                var (type, referenceComparer) = item;
                TestCollection(type, referenceComparer, genericCollectionType, false);
                TestCollection(type, referenceComparer, genericCollectionType, true);
            });
        }

        private static void TestNullableCollection(Type genericCollectionType = null)
        {
            foreach (var (nullableType, nullableComparer) in TestTypes.NullableTypes) {
                TestCollection(nullableType, nullableComparer, genericCollectionType, false);
                TestCollection(nullableType, nullableComparer, genericCollectionType, true);
            }
        }

        private static void TestCollection(Type objectType, IComparer itemComparer, Type genericCollectionType, bool sort)
        {
            var collectionType = genericCollectionType == null
                ? objectType.MakeArrayType()
                : genericCollectionType.MakeGenericType(objectType);

            var comparerType = typeof(CollectionComparer<>).MakeGenericType(objectType);
            var constructor = comparerType.GetConstructor(new[] { typeof(IComparer<>).MakeGenericType(objectType), typeof(bool) });
            var comparer = (IComparer)constructor.Invoke(new object[] { itemComparer, sort });

            new GenericTests().GenericTest(collectionType, comparer, sort, Constants.SmallCount);
        }

        [Fact]
        public void Compare_arrays_directly()
        {
            TestCollection();
        }

        [Fact]
        public void Compare_arrays_of_nullables_directly()
        {
            TestNullableCollection();
        }

        [Fact]
        public void Compare_enumerables_directly()
        {
            TestCollection(typeof(IEnumerable<>));
        }

        [Fact]
        public void Compare_enumerables_of_nullables_directly()
        {
            TestNullableCollection(typeof(IEnumerable<>));
        }

        [Fact]
        public void Compare_nullable_types_directly()
        {
            foreach (var (nullableType, nullableComparer) in TestTypes.NullableTypes) {
                new GenericTests().GenericTest(nullableType, nullableComparer, false, Constants.SmallCount);
            }
        }

        [Fact]
        public void Compare_types_directly()
        {
            Parallel.ForEach(TestTypes.Types, item => {
                var (type, referenceComparer) = item;
                new GenericTests().GenericTest(type, referenceComparer, false, Constants.SmallCount);
            });
        }

        [Fact]
        public void Should_use_delayed_comparison()
        {
            var x = Fixture.CreateMany<SampleStruct<EnumSmall?>?>().ToArray();
            var y = Fixture.CreateMany<SampleStruct<EnumSmall?>?>().ToArray();

            var referenceComparer = new CollectionComparer<SampleStruct<EnumSmall?>?>(new NullableComparer<SampleStruct<EnumSmall?>>(new SampleStructComparer<EnumSmall?>()));
            var comparer = new ComparerBuilder().GetComparer<object>();

            var expected = referenceComparer.Compare(x, y).Normalize();
            var actual = comparer.Compare(x, y).Normalize();

            actual.Should().Be(expected);
        }
    }
}
