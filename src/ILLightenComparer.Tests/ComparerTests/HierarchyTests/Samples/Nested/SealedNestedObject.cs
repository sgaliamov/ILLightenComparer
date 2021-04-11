using System;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested
{
    public sealed class SealedNestedObject : BaseNestedObject
    {
        public DeepNestedObject DeepNestedField;
        public DeepNestedObject DeepNestedProperty { get; set; }

        public override int CompareTo(object obj)
        {
            if (obj is null) {
                return 1;
            }

            if (ReferenceEquals(this, obj)) {
                return 0;
            }

            return obj is SealedNestedObject other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(SealedNestedObject)}.");
        }

        private int CompareTo(SealedNestedObject other)
        {
            var compare = DeepNestedObject.Comparer.Compare(DeepNestedField, other.DeepNestedField);
            if (compare != 0) {
                return compare;
            }

            compare = DeepNestedObject.Comparer.Compare(DeepNestedProperty, other.DeepNestedProperty);
            if (compare != 0) {
                return compare;
            }

            return base.CompareTo(other);
        }
    }
}
