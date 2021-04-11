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

namespace ILLightenComparer.Tests.EqualityTests
{
    internal sealed class GenericTests
    {
        private static readonly IFixture Fixture = FixtureBuilder.GetInstance();

        private static MethodInfo GetTestMethod(Type objType) => typeof(GenericTests)
                                                                 .GetGenericMethod(nameof(Test), BindingFlags.Instance | BindingFlags.NonPublic)
                                                                 .MakeGenericMethod(objType);

        private static void Comparison_of_null_with_object_produces_false<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer)
        {
            if (!typeof(T).IsClass && !typeof(T).IsNullable()) {
                return;
            }

            var obj = Fixture.Create<T>();

            using (new AssertionScope()) {
                referenceComparer.Equals(default, obj).Should().BeFalse();
                typedComparer.Equals(default, obj).Should().BeFalse();
            }
        }

        private static void Comparison_of_object_with_null_produces_false<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer)
        {
            if (!typeof(T).IsClass && !typeof(T).IsNullable()) {
                return;
            }

            var obj = Fixture.Create<T>();

            using (new AssertionScope()) {
                referenceComparer.Equals(obj, default).Should().BeFalse();
                typedComparer.Equals(obj, default).Should().BeFalse();
            }
        }

        private static void Comparison_when_both_null_produces_true<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer)
        {
            if (!typeof(T).IsClass && !typeof(T).IsNullable()) {
                return;
            }

            using (new AssertionScope()) {
                referenceComparer.Equals(default, default).Should().BeTrue();
                typedComparer.Equals(default, default).Should().BeTrue();
                referenceComparer.GetHashCode(default).Should().Be(0);
                typedComparer.GetHashCode(default).Should().Be(0);
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
                                     .GetGenericMethod(nameof(Enumerable.AsEnumerable), BindingFlags.Static | BindingFlags.Public)
                                     .MakeGenericMethod(elementType);

            foreach (var item in (IEnumerable)result) {
                var parameters = ThreadSafeRandom.NextDouble() < Constants.NullProbability
                    ? new[] { (object)null }
                    : new[] { item };

                addMethod.Invoke(list, parameters);
            }

            return (T)asEnumerableMethod.Invoke(null, new[] { list });
        }

        private readonly bool _compareHashes;
        private readonly IComparerBuilder _comparerBuilder;
        private readonly bool _sort;

        public GenericTests(bool sort, bool compareHashes = true, IComparerBuilder comparerBuilder = null)
        {
            _sort = sort;
            _compareHashes = compareHashes;
            _comparerBuilder = comparerBuilder;
        }

        private void Comparison_with_itself_produces_true<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer)
        {
            var obj = Fixture.Create<T>();
            var actual = typedComparer.GetHashCode(obj);

            using (new AssertionScope()) {
                var message = $"\n{string.Join("\n", obj.ObjectToArray())}";
                referenceComparer.Equals(obj, obj).Should().BeTrue(message);
                typedComparer.Equals(obj, obj).Should().BeTrue(message);

                if (_compareHashes) {
                    var expected = referenceComparer.GetHashCode(obj);
                    actual.Should().Be(expected, message);
                }
            }
        }

        private void Comparison_with_same_produces_true<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer)
        {
            var obj = Fixture.Create<T>();
            var clone = obj.DeepClone();

            using (new AssertionScope()) {
                var message = $"\nobj:\n{string.Join("\n", obj.ObjectToArray())}\nclone:\n{string.Join("\n", clone.ObjectToArray())}";
                referenceComparer.Equals(obj, clone).Should().BeTrue(message);
                typedComparer.Equals(obj, clone).Should().BeTrue(message);

                if (_compareHashes) {
                    typedComparer.GetHashCode(obj).Should().Be(referenceComparer.GetHashCode(clone), message);
                }
            }
        }

        private void Comparisons_work_identical<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer, int times)
        {
            var type = typeof(T);
            for (var i = 0; i < times; i++) {
                var x = Create<T>();
                var y = Create<T>();

                var expectedEquals = referenceComparer.Equals(x, y);
                var actualEquals = typedComparer.Equals(x, y);

                var message = $"{type.FullName} should be supported.";
                actualEquals.Should().Be(expectedEquals, message);

                if (_compareHashes) {
                    var expectedHashX = referenceComparer.GetHashCode(x);
                    var expectedHashY = referenceComparer.GetHashCode(y);
                    var actualHashX = typedComparer.GetHashCode(x);
                    var actualHashY = typedComparer.GetHashCode(y);

                    actualHashX.Should().Be(expectedHashX, message);
                    actualHashY.Should().Be(expectedHashY, message);
                }
            }
        }

        public void GenericTest(Type type, IEqualityComparer referenceComparer, int times)
        {
            var method = type.GetOrAddProperty($"Equality_{nameof(GenericTest)}", () => GetTestMethod(type));
            var builder = _comparerBuilder ?? new ComparerBuilder();
            builder.Configure(c => c.SetDefaultCollectionsOrderIgnoring(_sort));
            method.Invoke(this, new object[] { builder, referenceComparer, times });
        }

        private void Mutate_class_members_and_test_comparison<T>(IEqualityComparer<T> referenceComparer, IEqualityComparer<T> typedComparer, int times)
        {
            if (typeof(T).IsValueType) {
                return;
            }

            if (typeof(T).GetGenericInterface(typeof(IEnumerable<>)) != null) {
                return;
            }

            for (var i = 0; i < times; i++) {
                var original = Fixture.Create<T>();
                foreach (var mutant in Fixture.CreateMutants(original)) {
                    var because = $":\n{typeof(T)}\n{original}\n{mutant}\n{referenceComparer.GetType()}\n";
                    using (new AssertionScope()) {
                        referenceComparer.Equals(mutant, original).Should().BeFalse(because);
                        typedComparer.Equals(mutant, original).Should().BeFalse(because);

                        if (_compareHashes) {
                            typedComparer.GetHashCode(mutant).Should().Be(referenceComparer.GetHashCode(mutant), because);
                            typedComparer.GetHashCode(original).Should().Be(referenceComparer.GetHashCode(original), because);
                        }
                    }
                }
            }
        }

        private void Test<T>(IEqualityComparerProvider comparersBuilder, IEqualityComparer<T> referenceComparer, int times)
        {
            if (referenceComparer == null) {
                referenceComparer = EqualityComparer<T>.Default;
            }

            var typedComparer = comparersBuilder.GetEqualityComparer<T>();

            Parallel.Invoke(
                () => {
                    Comparison_of_null_with_object_produces_false(referenceComparer, typedComparer);
                    Comparison_of_object_with_null_produces_false(referenceComparer, typedComparer);
                    Comparison_when_both_null_produces_true(referenceComparer, typedComparer);
                    Comparison_with_itself_produces_true(referenceComparer, typedComparer);
                    Comparison_with_same_produces_true(referenceComparer, typedComparer);
                },
                () => Comparisons_work_identical(referenceComparer, typedComparer, times),
                () => Mutate_class_members_and_test_comparison(referenceComparer, typedComparer, times)
            );
        }
    }
}
