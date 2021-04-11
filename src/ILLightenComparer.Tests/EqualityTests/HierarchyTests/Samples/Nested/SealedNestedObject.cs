using System.Collections.Generic;
using ILLightenComparer.Tests.EqualityComparers;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested
{
    public sealed class SealedNestedObject : BaseNestedObject
    {
        public DeepNestedObject DeepNestedField;
        public DeepNestedObject DeepNestedProperty { get; set; }

        public override bool Equals(object obj) => Equals((SealedNestedObject)obj);

        public bool Equals(SealedNestedObject other) =>
            other != null
            && base.Equals(other)
            && EqualityComparer<DeepNestedObject>.Default.Equals(DeepNestedField, other.DeepNestedField)
            && EqualityComparer<DeepNestedObject>.Default.Equals(DeepNestedProperty, other.DeepNestedProperty);

        public override int GetHashCode() => HashCodeCombiner.Combine<object>(Text, Key, DeepNestedField, DeepNestedProperty);
    }
}
