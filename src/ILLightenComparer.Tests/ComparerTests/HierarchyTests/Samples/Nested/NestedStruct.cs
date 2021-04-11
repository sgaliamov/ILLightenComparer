using System;
using System.Collections.Generic;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested
{
    public struct NestedStruct
    {
        public ulong Property { get; set; }
        public ulong? NullableProperty { get; set; }

        public static RelationalComparer Comparer { get; } = new();

        public sealed class RelationalComparer : IComparer<NestedStruct>
        {
            public int Compare(NestedStruct x, NestedStruct y)
            {
                var valueComparison = x.Property.CompareTo(y.Property);
                if (valueComparison != 0) {
                    return valueComparison;
                }

                return Nullable.Compare(x.NullableProperty, y.NullableProperty);
            }

            public int Compare(NestedStruct? x, NestedStruct? y)
            {
                if (x.HasValue) {
                    if (y.HasValue) {
                        return Compare(x.Value, y.Value);
                    }

                    return 1;
                }

                if (y.HasValue) {
                    return -1;
                }

                return 0;
            }
        }
    }
}
