using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Force.DeepCloner;
using ILLightenComparer.Tests.EqualityTests.CycleTests.Samples;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests.CycleTests
{
    public sealed class CycledObjectsTests
    {
        private readonly IComparerBuilder _builder = new ComparerBuilder();

        private readonly Fixture _fixture;

        public CycledObjectsTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private IEqualityComparer<SelfSealed> ComparerSelfSealed => _builder.For<SelfSealed>().GetEqualityComparer();
        private IEqualityComparer<SelfOpened> ComparerSelfOpened => _builder.For<SelfOpened>().GetEqualityComparer();
        private IEqualityComparer<OneSealed> ComparerForOneSealed => _builder.For<OneSealed>().GetEqualityComparer();

        [Fact]
        public void Comparison_with_cycle_on_types_level_only()
        {
            var x = _fixture.Create<OneSealed>();
            var y = x.DeepClone();

            y.Value = (sbyte)(x.Value + 1);
            x.Two.Three.One = _fixture.Build<OneSealed>().Without(x => x.Two).Create();
            y.Two.Three.One = _fixture.Build<OneSealed>().Without(x => x.Two).Create();

            var hashX = ComparerForOneSealed.GetHashCode(x);
            var hashY = ComparerForOneSealed.GetHashCode(y);
            var equals = ComparerForOneSealed.Equals(x, y);

            using (new AssertionScope()) {
                equals.Should().BeFalse();
                hashX.Should().NotBe(0);
                hashY.Should().NotBe(0);
            }
        }

        [Fact]
        public void Cross_reference_should_fail()
        {
            var x = new SelfSealed();
            var y = new SelfSealed {
                First = x,
                Second = x
            };
            x.First = y;
            x.Second = y;

            using (new AssertionScope()) {
                ComparerSelfSealed.Equals(x, y).Should().BeTrue();
                ComparerSelfSealed.GetHashCode(x).Should().Be(ComparerSelfSealed.GetHashCode(y));
            }
        }

        [Fact]
        public void Cycle_detection_in_multiple_threads_works()
        {
            Helper.Parallel(() => {
                var comparer = new ComparerBuilder().GetEqualityComparer<OneSealed>();

                var one = _fixture.Create<OneSealed>();
                var other = _fixture.Create<OneSealed>();
                one.Two.Three.One = one;
                other.Two.Three.One = other;

                using (new AssertionScope()) {
                    comparer.Equals(one, other).Should().BeFalse();
                    comparer.GetHashCode(one).Should().NotBe(comparer.GetHashCode(other));
                }
            });
        }

        [Fact]
        public void Detects_cycle_handles_equalty_by_value()
        {
            var one = new SelfSealed { Value = 1 };
            one.Second = new SelfSealed {
                First = one,
                Value = 1
            };
            /*
                  1-1
                 / \
                N   2-1
            cycle: / \
                  1-1 N
                 / \ 
                N   2-1
            */

            var other = new SelfSealed {
                Second = new SelfSealed {
                    First = new SelfSealed {
                        Second = new SelfSealed { Value = 0 },
                        Value = 4
                    },
                    Value = 4
                },
                Value = 3
            };
            /*
                  3-3
                 / \
                N   4-4
                   / \
                  5-4 N
                 / \
                N   6-6
            */

            using (new AssertionScope()) {
                ComparerSelfSealed.GetHashCode(one).Should().Be(-2015758974);
                ComparerSelfSealed.GetHashCode(other).Should().Be(-1064642813);
                ComparerSelfSealed.Equals(one, other).Should().BeFalse();
            }
        }

        [Fact]
        public void Detects_cycle_on_second_member()
        {
            var one = new SelfSealed { Value = 1 };
            one.Second = new SelfSealed {
                First = one,
                Value = 2
            };
            /*
                  1
                 / \
                N   2
            cycle: / \
                  1   N
                 / \ 
                N   2
            */

            var other = new SelfSealed {
                Second = new SelfSealed {
                    First = new SelfSealed { Value = 3 },
                    Value = 4
                },
                Value = 5
            };
            /*
                  3
                 / \
                N   4
                   / \
                  5   N
                 / \
                N   N difference here: 2 > N
            */

            using (new AssertionScope()) {
                ComparerSelfSealed.GetHashCode(one).Should().Be(-2015759071);
                ComparerSelfSealed.GetHashCode(other).Should().Be(-1670556025);
                ComparerSelfSealed.Equals(one, other).Should().BeFalse();
            }
        }

        [Fact]
        public void Hasing_should_fail_because_of_generating_comparers_for_two_dependent_classes()
        {
            var one = _fixture.Create<OneSealed>();
            var other = _fixture.Create<OneSealed>();
            one.Two.Three.One = one;
            other.Two.Three.One = one;

            using (new AssertionScope()) {
                ComparerForOneSealed.Equals(one, other).Should().BeFalse();
                ComparerForOneSealed.GetHashCode(one).Should().NotBe(ComparerForOneSealed.GetHashCode(other));
            }
        }

        [Fact]
        public void No_cycle_when_identical_objects_in_collection()
        {
            var one = new SelfSealed { Value = 1 };
            one.Second = new SelfSealed {
                First = one,
                Value = 1
            };
            var two = new SelfSealed { Value = 1 };
            two.Second = new SelfSealed {
                First = two,
                Value = 1
            };
            var x = new[] {
                one,
                one,
                one
            };

            var other = new SelfSealed {
                Second = new SelfSealed {
                    First = new SelfSealed {
                        Value = 4
                    },
                    Value = 4
                },
                Value = 3
            };
            var y = new[] {
                two,
                two,
                other
            };

            var comparer = new ComparerBuilder().For<SelfSealed>().GetEqualityComparer<SelfSealed[]>();

            using (new AssertionScope()) {
                comparer.Equals(x, y).Should().BeFalse();
                comparer.GetHashCode(x).Should().Be(271135911);
                comparer.GetHashCode(y).Should().Be(194708355);
            }
        }

        [Fact]
        public void Object_with_bigger_cycle_is_bigger()
        {
            var one = new SelfSealed();
            one.First = new SelfSealed {
                First = new SelfSealed {
                    First = one
                }
            };
            var other = new SelfSealed { First = one };

            using (new AssertionScope()) {
                ComparerSelfSealed.GetHashCode(one).Should().Be(1342073958);
                ComparerSelfSealed.GetHashCode(other).Should().Be(-826099324);
                ComparerSelfSealed.Equals(one, other).Should().BeFalse();
            }
        }

        [Fact]
        public void Opened_class_comparer_uses_context_compare_method()
        {
            var one = _fixture.Create<SelfOpened>();
            one.Self = one;
            var other = _fixture.Create<SelfOpened>();
            other.Self = other;

            using (new AssertionScope()) {
                ComparerSelfOpened.Equals(one, other).Should().BeFalse();
                ComparerSelfOpened.GetHashCode(one).Should().NotBe(ComparerSelfOpened.GetHashCode(other));
            }
        }

        [Fact]
        public void When_sealed_comparable_has_member_with_cycle()
        {
            var comparer = _builder.For<ComparableChildObject<OneSealed>>().GetEqualityComparer();
            ComparableChildObject<OneSealed>.ChildComparer = _builder.GetComparer<OneSealed>();

            var one = _fixture.Create<OneSealed>();
            var other = _fixture.Create<OneSealed>();
            one.Value = other.Value;
            one.Two.Three.One = one;
            other.Two.Three.One = _fixture.Create<OneSealed>();
            other.Two.Three.One.Value = one.Value;
            other.Two.Three.One.Two.Three.One = other;
            var x = new ComparableChildObject<OneSealed> {
                ChildField = null,
                ChildProperty = other
            };
            var y = new ComparableChildObject<OneSealed> {
                ChildField = null,
                ChildProperty = one
            };

            using (new AssertionScope()) {
                comparer.Equals(x, y).Should().BeFalse();
                comparer.GetHashCode(x).Should().NotBe(comparer.GetHashCode(y));
            }
        }
    }
}
