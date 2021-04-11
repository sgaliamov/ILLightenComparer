using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.EqualityTests.CycleTests.Samples;
using ILLightenComparer.Tests.Samples;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests.CycleTests
{
    public sealed class CycledStructTests
    {
        private readonly IComparerBuilder _builder;

        public CycledStructTests()
        {
            _builder = new ComparerBuilder(
                           config => config.DefineMembersOrder<CycledStruct>(
                               order => order.Member(o => o.Property)
                                             .Member(o => o.FirstObject)
                                             .Member(o => o.SecondObject)))
                       .For<CycledStructObject>(
                           config => config.DefineMembersOrder(
                               order => order.Member(o => o.TextField)
                                             .Member(o => o.FirstStruct)
                                             .Member(o => o.SecondStruct)))
                       .Builder;
        }

        private IEqualityComparer<CycledStruct> ComparerStruct => _builder.GetEqualityComparer<CycledStruct>();
        private IEqualityComparer<CycledStructObject> ComparerObject => _builder.GetEqualityComparer<CycledStructObject>();

        [Fact]
        public void Detects_cycle_in_object()
        {
            var one = new CycledStructObject();
            one.FirstStruct = new CycledStruct { SecondObject = one };

            var other = new CycledStructObject();
            other.FirstStruct = new CycledStruct { SecondObject = other };

            using (new AssertionScope()) {
                ComparerObject.Equals(one, other).Should().BeTrue();
                ComparerObject.GetHashCode(one).Should().Be(ComparerObject.GetHashCode(other));
            }
        }

        [Fact]
        public void Detects_cycle_in_struct()
        {
            var one = new CycledStruct {
                FirstObject = new CycledStructObject()
            };
            one.FirstObject.SecondStruct = one;

            var other = new CycledStruct {
                FirstObject = new CycledStructObject()
            };
            other.FirstObject.SecondStruct = other;

            using (new AssertionScope()) {
                ComparerStruct.Equals(one, other).Should().BeTrue();
                ComparerStruct.GetHashCode(one).Should().Be(ComparerStruct.GetHashCode(other));
            }
        }

        [Fact]
        public void Detects_cycle_on_first_member()
        {
            var one = new CycledStructObject();
            /*
                      1
                     / \
                    vX  vX
                   / |  | \ 
                  N  N  N  N
            */

            var other = new CycledStructObject();
            other.FirstStruct = new CycledStruct {
                FirstObject = other
            };
            /*
                      2
                     / \
                    v1  vX
                   / |  | \ 
                  2  N  N  N
            */

            using (new AssertionScope()) {
                ComparerObject.Equals(one, other).Should().BeFalse();
                ComparerObject.GetHashCode(one).Should().Be(0);
                ComparerObject.GetHashCode(other).Should().Be(-1940015289);
            }
        }

        [Fact]
        public void Detects_cycle_on_second_member()
        {
            var one = new CycledStruct { SecondObject = new CycledStructObject() };
            one.SecondObject.FirstStruct = one;
            /*
                  v1
                 /  \
                N    1
                   /  \
                  v1   vX
                 / |   | \
                N  1   N  N
            */

            var other = new CycledStruct {
                SecondObject = new CycledStructObject()
            };
            /*
                  v2
                 /  \
                N    2
                   /  \
                  v2   VY
                 / |   | \
                N  N   N  N
            */

            using (new AssertionScope()) {
                ComparerStruct.Equals(one, other).Should().BeFalse();
                ComparerStruct.GetHashCode(one).Should().Be(193377063);
                ComparerStruct.GetHashCode(other).Should().Be(193376997);
            }
        }

        [Fact]
        public void Self_sealed_struct_should_handle_cycle()
        {
            var comparer = _builder.GetEqualityComparer<SelfStruct<Guid>>();
            var x = new SelfStruct<Guid> {
                Key = Guid.NewGuid(),
                Value = Guid.NewGuid()
            };

            using (new AssertionScope()) {
                comparer.Equals(x, x).Should().BeTrue();
                comparer.GetHashCode(x).Should().Be(HashCodeCombiner.Combine<object>(x.Key, 1, x.Value));
            }
        }
    }
}
