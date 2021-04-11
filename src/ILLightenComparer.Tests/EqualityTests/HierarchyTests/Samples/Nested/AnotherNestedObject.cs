using ILLightenComparer.Tests.EqualityComparers;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested
{
    public sealed class AnotherNestedObject : BaseNestedObject
    {
        public string Value { get; set; }

        public override bool Equals(object obj) => Equals((AnotherNestedObject)obj);

        public bool Equals(AnotherNestedObject other) =>
            other != null
            && base.Equals(other)
            && Value == other.Value;

        public override int GetHashCode() => HashCodeCombiner.Combine<object>(Value, Key, Text);
    }
}
