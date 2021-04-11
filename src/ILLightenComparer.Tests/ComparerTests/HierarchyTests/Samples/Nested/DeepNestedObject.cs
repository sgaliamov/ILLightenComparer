using System.Collections.Generic;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested
{
    public class DeepNestedObject
    {
        public static IComparer<DeepNestedObject> Comparer { get; } = new RelationalComparer();
        public float FloatField;

        public float FloatProperty { get; set; }

        private sealed class RelationalComparer : IComparer<DeepNestedObject>
        {
            public int Compare(DeepNestedObject x, DeepNestedObject y)
            {
                if (ReferenceEquals(x, y)) {
                    return 0;
                }

                if (y is null) {
                    return 1;
                }

                if (x is null) {
                    return -1;
                }

                var floatFieldComparison = x.FloatField.CompareTo(y.FloatField);
                if (floatFieldComparison != 0) {
                    return floatFieldComparison;
                }

                return x.FloatProperty.CompareTo(y.FloatProperty);
            }
        }
    }
}
