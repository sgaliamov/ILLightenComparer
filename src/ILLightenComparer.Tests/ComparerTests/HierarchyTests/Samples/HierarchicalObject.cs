using System.Collections;
using System.Collections.Generic;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Samples;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples
{
    public sealed class HierarchicalObject
    {
        public static RelationalComparer Comparer { get; } = new();
        public ComparableBaseObject<EnumSmall> ComparableField;
        public SealedNestedObject NestedField;
        public NestedStruct? NestedNullableStructField;
        public NestedStruct NestedStructField;

        public SealedNestedObject FirstProperty { get; set; }
        public NestedStruct? NestedNullableStructProperty { get; set; }
        public NestedStruct NestedStructProperty { get; set; }
        public SealedNestedObject SecondProperty { get; set; }
        public int Value { get; set; }

        public sealed class RelationalComparer : IComparer<HierarchicalObject>, IComparer
        {
            public int Compare(object x, object y) => Compare(x as HierarchicalObject, y as HierarchicalObject);

            public int Compare(HierarchicalObject x, HierarchicalObject y)
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

                var compare =
                    Comparer<ComparableBaseObject<EnumSmall>>.Default.Compare(x.ComparableField, y.ComparableField);
                if (compare != 0) {
                    return compare;
                }

                compare = x.Value.CompareTo(y.Value);
                if (compare != 0) {
                    return compare;
                }

                compare = Comparer<SealedNestedObject>.Default.Compare(x.FirstProperty, y.FirstProperty);
                if (compare != 0) {
                    return compare;
                }

                compare = Comparer<SealedNestedObject>.Default.Compare(x.SecondProperty, y.SecondProperty);
                if (compare != 0) {
                    return compare;
                }

                compare = Comparer<SealedNestedObject>.Default.Compare(x.NestedField, y.NestedField);
                if (compare != 0) {
                    return compare;
                }

                compare = NestedStruct.Comparer.Compare(x.NestedStructField, y.NestedStructField);
                if (compare != 0) {
                    return compare;
                }

                compare = NestedStruct.Comparer.Compare(x.NestedNullableStructField, y.NestedNullableStructField);
                if (compare != 0) {
                    return compare;
                }

                compare = NestedStruct.Comparer.Compare(x.NestedStructProperty, y.NestedStructProperty);
                if (compare != 0) {
                    return compare;
                }

                compare = NestedStruct.Comparer.Compare(x.NestedNullableStructProperty, y.NestedNullableStructProperty);
                if (compare != 0) {
                    return compare;
                }

                return 0;
            }
        }
    }
}
