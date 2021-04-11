using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests
{
    public sealed class MultiThreadingTests
    {
        private static IEqualityComparer<AbstractMembers> CreateComparer()
        {
            return new ComparerBuilder()
                   .For<AnotherNestedObject>(
                       c => c.DefineMembersOrder(
                           order => order.Member(o => o.Value)
                                         .Member(o => o.Key)
                                         .Member(o => o.Text)))
                   .For<AbstractMembers>()
                   .GetEqualityComparer();
        }

        private readonly Fixture _fixture;

        public MultiThreadingTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void Generate_comparer_for_not_sealed_member_in_parallel_still_works()
        {
            Helper.Parallel(() => {
                var one = new AbstractMembers {
                    NotSealedProperty = _fixture.Create<AnotherNestedObject>()
                };
                var other = new AbstractMembers {
                    NotSealedProperty = _fixture.Create<AnotherNestedObject>()
                };

                var expectedHashX = one.GetHashCode();
                var expectedHashY = other.GetHashCode();
                var expectedEquals = one.Equals(other);

                var comparer = CreateComparer();
                Helper.Parallel(() => {
                    var hashX = comparer.GetHashCode(one);
                    var hashY = comparer.GetHashCode(other);
                    var equals = comparer.Equals(one, other);

                    using (new AssertionScope()) {
                        equals.Should().Be(expectedEquals);
                        hashX.Should().Be(expectedHashX);
                        hashY.Should().Be(expectedHashY);
                    }
                });
            });
        }
    }
}
