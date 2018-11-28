﻿using System.Collections.Generic;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests
{
    public sealed class DeepNestedObject
    {
        public float FloatField;

        public static IComparer<DeepNestedObject> Comparer { get; } =
            new FloatFieldFloatPropertyRelationalComparer();

        public float FloatProperty { get; set; }

        private sealed class FloatFieldFloatPropertyRelationalComparer : IComparer<DeepNestedObject>
        {
            public int Compare(DeepNestedObject x, DeepNestedObject y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (ReferenceEquals(null, y))
                {
                    return 1;
                }

                if (ReferenceEquals(null, x))
                {
                    return -1;
                }

                var floatFieldComparison = x.FloatField.CompareTo(y.FloatField);
                if (floatFieldComparison != 0)
                {
                    return floatFieldComparison;
                }

                return x.FloatProperty.CompareTo(y.FloatProperty);
            }
        }
    }
}