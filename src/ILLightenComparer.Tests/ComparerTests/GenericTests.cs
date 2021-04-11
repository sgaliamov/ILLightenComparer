using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Force.DeepCloner;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.ComparerTests
{
    internal sealed class GenericTests
    {
        private static readonly IFixture Fixture = FixtureBuilder.GetInstance();

        private static MethodInfo GetTestMethod(Type objType) =>
            typeof(GenericTests)
                .GetGenericMethod(nameof(Test), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(objType);

        private static void Test<T>(IComparerProvider comparersBuilder, IComparer<T> referenceComparer, int times, int count)
        {
            if (referenceComparer == null) {
                referenceComparer = Helper.DefaultComparer<T>();
            }

            var typedComparer = comparersBuilder.GetComparer<T>();

            Parallel.Invoke(
                () => {
                    Comparison_of_null_with_object_produces_negative_value(referenceComparer, typedComparer);
                    Comparison_of_object_with_null_produces_positive_value(referenceComparer, typedComparer);
                    Comparison_when_both_null_produces_0(referenceComparer, typedComparer);
                    Comparison_with_itself_produces_0(referenceComparer, typedComparer);
                    Comparison_with_same_produces_0(referenceComparer, typedComparer);
                },
                () => Comparisons_work_identical(referenceComparer, typedComparer, times),
                () => Sorting_must_work_the_same_as_for_reference_comparer(referenceComparer, typedComparer, count),
                () => Mutate_class_members_and_test_comparison(referenceComparer, typedComparer)
            );
        }

        private static void Comparison_of_null_with_object_produces_negative_value<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer)
        {
            if (!typeof(T).IsClass && !typeof(T).IsNullable()) {
                return;
            }

            var obj = Fixture.Create<T>();

            using (new AssertionScope()) {
                referenceComparer.Compare(default, obj).Should().BeNegative();
                typedComparer.Compare(default, obj).Should().BeNegative();
            }
        }

        private static void Comparison_of_object_with_null_produces_positive_value<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer)
        {
            if (!typeof(T).IsClass && !typeof(T).IsNullable()) {
                return;
            }

            var obj = Fixture.Create<T>();

            using (new AssertionScope()) {
                referenceComparer.Compare(obj, default).Should().BePositive();
                typedComparer.Compare(obj, default).Should().BePositive();
            }
        }

        private static void Comparison_when_both_null_produces_0<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer)
        {
            if (!typeof(T).IsClass && !typeof(T).IsNullable()) {
                return;
            }

            using (new AssertionScope()) {
                referenceComparer.Compare(default, default).Should().Be(0);
                typedComparer.Compare(default, default).Should().Be(0);
            }
        }

        private static void Comparison_with_itself_produces_0<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer)
        {
            var obj = Fixture.Create<T>();

            using (new AssertionScope()) {
                referenceComparer.Compare(obj, obj).Should().Be(0);
                typedComparer.Compare(obj, obj).Should().Be(0);
            }
        }

        private static void Comparison_with_same_produces_0<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer)
        {
            var obj = Fixture.Create<T>();
            var clone = obj.DeepClone();

            using (new AssertionScope()) {
                referenceComparer.Compare(obj, clone).Should().Be(0);
                typedComparer.Compare(obj, clone).Should().Be(0);
            }
        }

        private static void Mutate_class_members_and_test_comparison<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer)
        {
            if (typeof(T).IsValueType) {
                return;
            }

            if (typeof(T).GetGenericInterface(typeof(IEnumerable<>)) != null) {
                return;
            }

            var original = Fixture.Create<T>();
            foreach (var mutant in Fixture.CreateMutants(original)) {
                using (new AssertionScope()) {
                    referenceComparer.Compare(mutant, original).Should().NotBe(0);
                    typedComparer.Compare(mutant, original).Should().NotBe(0);
                }
            }
        }

        private static void Sorting_must_work_the_same_as_for_reference_comparer<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer, int count)
        {
            var original = CreateMany<T>(count).ToArray();

            var copy0 = original.DeepClone();
            var copy1 = original.DeepClone();

            Array.Sort(copy0, referenceComparer);
            Array.Sort(copy1, typedComparer);

            copy1.ShouldBeSameOrder(copy0);
            copy1.ShouldBeSameOrder(copy0);
        }

        private static void Comparisons_work_identical<T>(IComparer<T> referenceComparer, IComparer<T> typedComparer, int times)
        {
            var type = typeof(T);
            for (var i = 0; i < times; i++) {
                var x = Create<T>();
                var y = Create<T>();

                var expected = referenceComparer.Compare(x, y).Normalize();
                var actual = typedComparer.Compare(x, y).Normalize();

                var message = $"{type.FullName} should be supported.\n"
                              + $"x: {x.ToJson()},\n"
                              + $"y: {y.ToJson()}";

                actual.Should().Be(expected, message);
            }
        }

        private static T Create<T>()
        {
            var type = typeof(T);

            if ((!type.IsValueType || type.IsNullable()) && ThreadSafeRandom.NextDouble() < Constants.NullProbability) {
                return default;
            }

            var result = Fixture.Create<T>();

            if (type.IsValueType) {
                return result;
            }

            if (type.IsArray && result is IList list) {
                return (T)AppendNulls(list); // todo: 1. test
            }

            var genericInterface = type.GetGenericInterface(typeof(IEnumerable<>));
            if (genericInterface != null) {
                return AppendNulls(result, genericInterface);
            }

            return result;
        }

        private static IList AppendNulls(IList list)
        {
            for (var i = 0; i < list.Count; i++) {
                if (ThreadSafeRandom.NextDouble() < Constants.NullProbability) {
                    list[i] = null;
                }
            }

            return list;
        }

        private static T AppendNulls<T>(T result, Type genericInterface)
        {
            var elementType = genericInterface.GetGenericArguments()[0];
            if (elementType.IsValueType && !elementType.IsNullable()) {
                return result;
            }

            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod(nameof(List<object>.Add), new[] { elementType });
            var asEnumerableMethod = typeof(Enumerable)
                                     .GetGenericMethod(nameof(Enumerable.AsEnumerable),
                                                       BindingFlags.Static | BindingFlags.Public)
                                     .MakeGenericMethod(elementType);

            foreach (var item in (IEnumerable)result) {
                var parameters = ThreadSafeRandom.NextDouble() < Constants.NullProbability
                    ? new[] { (object)null }
                    : new[] { item };

                addMethod.Invoke(list, parameters);
            }

            return (T)asEnumerableMethod.Invoke(null, new[] { list });
        }

        private static IEnumerable<T> CreateMany<T>(int count)
        {
            for (var j = 0; j < count; j++) {
                yield return Create<T>();
            }
        }

        private readonly IComparerBuilder _comparerBuilder;

        public GenericTests(IComparerBuilder comparerBuilder = null)
        {
            _comparerBuilder = comparerBuilder;
        }

        public void GenericTest(Type type, IComparer referenceComparer, bool sort, int times, int count = Constants.BigCount)
        {
            var method = type.GetOrAddProperty($"Comparer_{nameof(GenericTest)}", () => GetTestMethod(type));
            var builder = _comparerBuilder ?? new ComparerBuilder();
            builder.Configure(c => c.SetDefaultCollectionsOrderIgnoring(sort));
            method.Invoke(null, new object[] { builder, referenceComparer, times, count });
        }
    }
}
