using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples.Nested;

namespace ILLightenComparer.Tests.EqualityTests.HierarchyTests.Samples
{
    public class AbstractMembers
    {
        public INestedObject InterfaceField;
        public object ObjectField;

        public AbstractNestedObject AbstractProperty { get; set; }
        public BaseNestedObject NotSealedProperty { get; set; }

        public override bool Equals(object obj) => Equals((AbstractMembers)obj);

        public bool Equals(AbstractMembers other) =>
            other != null
            && (InterfaceField?.Equals(other.InterfaceField) ?? other.InterfaceField is null)
            && (ObjectField?.Equals(other.ObjectField) ?? other.ObjectField is null)
            && (AbstractProperty?.Equals((object)other.AbstractProperty) ?? other.AbstractProperty is null)
            && (NotSealedProperty?.Equals((object)other.NotSealedProperty) ?? other.NotSealedProperty is null);

        public override int GetHashCode() => HashCodeCombiner.Combine(InterfaceField, ObjectField, AbstractProperty, NotSealedProperty);
    }
}
