using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
{
    public sealed class BasicTests
    {
        private readonly IFixture _fixture = FixtureBuilder.GetInstance();

        [Fact]
        public void Delayed_comparison_on_sample_object()
        {
            var x = new SampleObject<EnumSmall?> { Field = null };
            var y = new SampleObject<EnumSmall?> { Field = EnumSmall.One };

            var comparer = new ComparerBuilder().GetComparer<object>();

            var actual = comparer.Compare(x, y);

            actual.Should().Be(-1);
        }

        [Fact]
        public void Empty_nullable_structs_should_be_equal()
        {
            var comparer = new ComparerBuilder().GetComparer<DummyStruct?>();

            _fixture.Create<DummyStruct>();

            var actual = comparer.Compare(new DummyStruct(), new DummyStruct());

            actual.Should().Be(0);
        }

        [Fact]
        public void Empty_object_should_be_equal()
        {
            var comparer = new ComparerBuilder().GetComparer<DummyObject>();

            var actual = comparer.Compare(new DummyObject(), new DummyObject());

            actual.Should().Be(0);
        }

        [Fact]
        public void Empty_structs_should_be_equal()
        {
            var comparer = new ComparerBuilder().GetComparer<DummyStruct>();

            var actual = comparer.Compare(new DummyStruct(), new DummyStruct());

            actual.Should().Be(0);
        }

        [Fact]
        public void Enumerable_structs_are_comparable()
        {
            var referenceComparer = new CollectionComparer<int>();
            var x = _fixture.Create<EnumerableStruct<int>>();
            var y = _fixture.Create<EnumerableStruct<int>>();
            var expected = referenceComparer.Compare(x, y);

            var comparer = new ComparerBuilder().GetComparer<EnumerableStruct<int>>();

            var result = comparer.Compare(x, y);

            result.Should().Be(expected);
        }

        [Fact]
        public void Enumerable_structs_with_nullables_are_comparable()
        {
            var referenceComparer =
                new CollectionComparer<SampleStruct<int?>?>(
                    new NullableComparer<SampleStruct<int?>>(new SampleStructComparer<int?>()));
            var comparer = new ComparerBuilder().GetComparer<EnumerableStruct<SampleStruct<int?>?>>();

            Helper.Parallel(() => {
                var x = _fixture.Create<EnumerableStruct<SampleStruct<int?>?>>();
                var y = _fixture.Create<EnumerableStruct<SampleStruct<int?>?>>();

                var expectedEquals = referenceComparer.Compare(x, y);
                var equals = comparer.Compare(x, y);

                using (new AssertionScope()) {
                    comparer.Compare(x, x).Should().Be(0);
                    equals.Should().Be(expectedEquals);
                }
            });
        }

        [Fact]
        public void Enumerables_are_not_equal()
        {
            var x = new List<int>(new[] { 1, 2, 3 });
            var y = new List<int>(new[] { 2, 3, 1 });

            var comparer = new ComparerBuilder().GetComparer<IEnumerable<int>>();

            var result = comparer.Compare(x, y);

            result.Should().Be(-1);
        }

        [Fact]
        public void Null_enumerator_pass()
        {
            var x = new EnumerableStruct<int>(null);
            var comparer = new ComparerBuilder().GetComparer<EnumerableStruct<int>>();

            Assert.Throws<NullReferenceException>(() => comparer.Compare(x, x));
            Assert.Throws<NullReferenceException>(() => comparer.Compare(default, default));
        }

        [Fact]
        public void Objects_are_identical_because_they_have_no_members()
        {
            var x = new object();
            var y = new object();

            var comparer = new ComparerBuilder().GetComparer<object>();

            var actual = comparer.Compare(x, y);

            actual.Should().Be(0);
        }

        [Fact]
        public void Test_all_basic_types()
        {
            foreach (var type in Helper.BasicTypes) {
                new GenericTests().GenericTest(type, null, false, Constants.SmallCount);
            }
        }
    }
}
