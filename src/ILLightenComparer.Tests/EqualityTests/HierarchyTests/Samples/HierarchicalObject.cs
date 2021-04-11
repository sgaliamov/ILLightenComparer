using System.Collections.Generic;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested;
using ILLightenComparer.Tests.Samples;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples
{
    public sealed class HierarchicalObject
    {
        public SampleEqualityBaseObject<EnumSmall> ComparableField;
        public SealedNestedObject NestedField;
        public NestedStruct? NestedNullableStructField;
        public NestedStruct NestedStructField;

        public SealedNestedObject FirstProperty { get; set; }
        public NestedStruct? NestedNullableStructProperty { get; set; }
        public NestedStruct NestedStructProperty { get; set; }
        public SealedNestedObject SecondProperty { get; set; }
        public int Value { get; set; }

        public override bool Equals(object obj) => Equals((HierarchicalObject)obj);

        public bool Equals(HierarchicalObject other) =>
            other != null
            && (ComparableField == null ? other.ComparableField is null : ComparableField.CompareTo(other.ComparableField) == 0)
            && EqualityComparer<SealedNestedObject>.Default.Equals(NestedField, other.NestedField)
            && EqualityComparer<NestedStruct?>.Default.Equals(NestedNullableStructField, other.NestedNullableStructField)
            && EqualityComparer<NestedStruct>.Default.Equals(NestedStructField, other.NestedStructField)
            && EqualityComparer<SealedNestedObject>.Default.Equals(FirstProperty, other.FirstProperty)
            && EqualityComparer<NestedStruct?>.Default.Equals(NestedNullableStructProperty, other.NestedNullableStructProperty)
            && EqualityComparer<NestedStruct>.Default.Equals(NestedStructProperty, other.NestedStructProperty)
            && EqualityComparer<SealedNestedObject>.Default.Equals(SecondProperty, other.SecondProperty)
            && Value == other.Value;

        public override int GetHashCode() => HashCodeCombiner.Combine<object>(
            ComparableField,
            Value,
            FirstProperty,
            SecondProperty,
            NestedField,
            NestedStructField,
            NestedNullableStructField,
            NestedStructProperty,
            NestedNullableStructProperty
        );
    }
}
