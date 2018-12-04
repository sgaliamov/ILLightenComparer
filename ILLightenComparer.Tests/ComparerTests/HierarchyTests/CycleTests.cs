﻿using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Cycle;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests
{
    public class CycleTests
    {
        [Fact]
        public void Cross_Reference_Should_Not_Fail()
        {
            var other = new SelfSealed();
            var one = new SelfSealed
            {
                First = other,
                Second = other
            };
            other.First = one;
            other.Second = one;

            var expected = SelfSealed.Comparer.Compare(one, other);
            var actual = ComparerSelfSealed.Compare(one, other);

            expected.Should().Be(0);
            actual.Should().Be(expected);
        }

        [Fact(Skip = "Implement structs comparison first.")]
        public void Cycle_In_Struct()
        {
            var nestedObject = new ObjectWithCycledStruct
            {
                Value = new CycledStruct
                {
                    Object = new ObjectWithCycledStruct()
                }
            };
            var cycledStruct = new CycledStruct
            {
                Object = nestedObject
            };
            nestedObject.Value.Object.Value = cycledStruct;

            //var expected = SelfSealed.Comparer.Compare(one, other);
            //var actual = ComparerSelfSealed.Compare(one, other);

            //actual.Should().Be(expected);
        }

        [Fact]
        public void Detects_Cycle_On_Second_Member_Loop()
        {
            var one = new SelfSealed();
            one.Second = new SelfSealed
            {
                First = one
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

            var other = new SelfSealed
            {
                Second = new SelfSealed
                {
                    First = new SelfSealed()
                }
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

            var expected = SelfSealed.Comparer.Compare(one, other);
            var actual = ComparerSelfSealed.Compare(one, other);

            expected.Should().Be(1);
            actual.Should().Be(expected);
        }

        [Fact(Skip = "Not implemented yet")]
        public void Nested_Sealed_Comparison_Should_Not_Fail()
        {
            var one = _fixture.Create<OneSealed>();

            var other = _fixture.Create<OneSealed>();

            //var expected = OneSealed.Comparer.Compare(one, other);
            //var actual = ComparerOneSealed.Compare(one, other);

            //actual.Should().Be(expected);
        }

        [Fact]
        public void Opened_Class_Comparer_Uses_Context_Compare_Method()
        {
            var one = _fixture.Create<SelfOpened>();
            one.Self = one;
            var other = _fixture.Create<SelfOpened>();
            other.Self = other;

            var expected = one.Value.CompareTo(other.Value);
            var actual = ComparerSelfOpened.Compare(one, other);

            actual.Should().Be(expected);
        }

        [Fact]
        public void Self_Sealed_Comparison_Should_Not_Fail()
        {
            var one = new SelfSealed();
            one.First = new SelfSealed
            {
                First = new SelfSealed
                {
                    First = one
                }
            };
            var other = _fixture.Create<SelfSealed>();
            other.First = one;

            var expected = SelfSealed.Comparer.Compare(one, other);
            var actual = ComparerSelfSealed.Compare(one, other);

            actual.Should().Be(expected);
        }

        private readonly Fixture _fixture = FixtureBuilder.GetInstance();

        private readonly IContextBuilder _builder =
            new ComparersBuilder()
                .DefineDefaultConfiguration(new ComparerSettings
                {
                    IncludeFields = true,
                    DetectCycles = true
                });

        private IComparer<SelfSealed> ComparerSelfSealed =>
            _builder.For<SelfSealed>()
                    .DefineConfiguration(new ComparerSettings
                    {
                        IgnoredMembers = new[] { nameof(SelfSealed.Id) }
                    })
                    .GetComparer();

        private IComparer<SelfOpened> ComparerSelfOpened =>
            _builder.For<SelfOpened>().GetComparer();

        //private IComparer<OneSealed> ComparerOneSealed =>
        //    _builder.For<OneSealed>()
        //            .DefineConfiguration(new ComparerSettings())
        //            .GetComparer();
    }
}
