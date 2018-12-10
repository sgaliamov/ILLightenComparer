﻿using System;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested
{
    public sealed class AnotherNestedObject : BaseNestedObject
    {
        public string Value { get; set; }

        public override int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is AnotherNestedObject other
                       ? CompareTo(other)
                       : throw new ArgumentException($"Object must be of type {nameof(AnotherNestedObject)}.");
        }

        private int CompareTo(AnotherNestedObject other)
        {
            var compare = string.Compare(Value, other.Value, StringComparison.Ordinal);
            if (compare != 0)
            {
                return compare;
            }

            return base.CompareTo(other);
        }
    }
}
