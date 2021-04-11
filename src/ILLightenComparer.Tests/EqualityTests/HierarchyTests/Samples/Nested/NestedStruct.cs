using System.Diagnostics.CodeAnalysis;
using ILLightenComparer.Tests.EqualityComparers;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [SuppressMessage("Usage", "CA2231:Overload operator equals on overriding value type Equals", Justification = "<Pending>")]
    public struct NestedStruct
    {
        public ulong Property { get; set; }
        public ulong? NullableProperty { get; set; }

        public override bool Equals(object obj) => Equals((NestedStruct)obj);

        public bool Equals(NestedStruct other) =>
            Property == other.Property
            && NullableProperty == other.NullableProperty;

        public override int GetHashCode() => HashCodeCombiner.Combine(Property, NullableProperty);
    }
}
