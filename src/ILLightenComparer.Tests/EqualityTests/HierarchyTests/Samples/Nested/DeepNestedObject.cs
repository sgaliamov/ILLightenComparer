using ILLightenComparer.Tests.EqualityComparers;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested
{
    public class DeepNestedObject
    {
        public float FloatField;
        public float FloatProperty { get; set; }

        public override bool Equals(object obj) => Equals((DeepNestedObject)obj);

        public bool Equals(DeepNestedObject other) =>
            other != null
            && FloatField == other.FloatField
            && FloatProperty == other.FloatProperty;

        public override int GetHashCode() => HashCodeCombiner.Combine(FloatField, FloatProperty);
    }
}
