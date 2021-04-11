using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using ILLightenComparer.Tests.ComparerTests.CycleTests.Samples;
using ILLightenComparer.Tests.Samples;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests.CycleTests
{
    public sealed class CycledStructTests
    {
        private readonly IComparerBuilder _builder;

        public CycledStructTests()
        {
            _builder = new ComparerBuilder(config =>
                                               config.DefineMembersOrder<CycledStruct>(
                                                   order => order.Member(o => o.Property)
                                                                 .Member(o => o.FirstObject)
                                                                 .Member(o => o.SecondObject)))
                       .For<CycledStructObject>(config =>
                                                    config.DefineMembersOrder(order =>
                                                                                  order.Member(o => o.TextField)
                                                                                       .Member(o => o.FirstStruct)
                                                                                       .Member(o => o.SecondStruct))
                                                          .IgnoreMember(o => o.Id))
                       .Builder;
        }

        public IComparer<CycledStruct> ComparerStruct => _builder.GetComparer<CycledStruct>();
        public IComparer<CycledStructObject> ComparerObject => _builder.GetComparer<CycledStructObject>();

        [Fact]
        public void Detects_cycle_in_object()
        {
            var one = new CycledStructObject();
            one.FirstStruct = new CycledStruct { SecondObject = one };

            var other = new CycledStructObject();
            other.FirstStruct = new CycledStruct { SecondObject = other };

            var expected = CycledStructObject.Comparer.Compare(one, other);
            var actual = ComparerObject.Compare(one, other);

            using (new AssertionScope()) {
                expected.Should().Be(0);
                actual.Should().Be(expected);
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

            var expected = CycledStruct.Comparer.Compare(one, other);
            var actual = ComparerStruct.Compare(one, other);

            using (new AssertionScope()) {
                expected.Should().Be(0);
                actual.Should().Be(expected);
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

            var expected = CycledStructObject.Comparer.Compare(one, other);
            var actual = ComparerObject.Compare(one, other);

            using (new AssertionScope()) {
                expected.Should().Be(-1);
                actual.Should().Be(expected);
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

            var expected = CycledStruct.Comparer.Compare(one, other);
            var actual = ComparerStruct.Compare(one, other);

            using (new AssertionScope()) {
                expected.Should().Be(1);
                actual.Should().Be(expected);
            }
        }

        [Fact]
        public void Sefl_sealed_struct_should_be_equal()
        {
            var comparer = _builder.GetComparer<SelfStruct<Guid>>();
            var x = new SelfStruct<Guid> {
                Key = Guid.NewGuid(),
                Value = Guid.NewGuid()
            };

            comparer.Compare(x, x).Should().Be(0);
        }
    }
}
