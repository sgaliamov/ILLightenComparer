using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests
{
    public sealed class MultiThreadingTests
    {
        private static IComparer<AbstractMembers> CreateComparer()
        {
            return new ComparerBuilder()
                   .For<AnotherNestedObject>(
                       c => c.DefineMembersOrder(
                           order => order.Member(o => o.Value)
                                         .Member(o => o.Key)
                                         .Member(o => o.Text)))
                   .For<AbstractMembers>()
                   .GetComparer();
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
            var one = new AbstractMembers {
                NotSealedProperty = _fixture.Create<AnotherNestedObject>()
            };

            Helper.Parallel(() => {
                var comparer = CreateComparer();

                var other = new AbstractMembers {
                    NotSealedProperty = _fixture.Create<AnotherNestedObject>()
                };

                var expected = AbstractMembers.Comparer.Compare(one, other).Normalize();

                Helper.Parallel(() => {
                    var actual = comparer.Compare(one, other).Normalize();
                    actual.Should().Be(expected);
                });
            });
        }
    }
}
