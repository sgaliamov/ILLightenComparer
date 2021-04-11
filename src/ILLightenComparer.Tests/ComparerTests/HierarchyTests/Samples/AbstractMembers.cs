using System;
using System.Collections.Generic;
using ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples
{
    public class AbstractMembers
    {
        public static IComparer<AbstractMembers> Comparer { get; } = new RelationalComparer();
        public INestedObject InterfaceField;
        public object ObjectField;

        public AbstractNestedObject AbstractProperty { get; set; }
        public BaseNestedObject NotSealedProperty { get; set; }

        private sealed class RelationalComparer : IComparer<AbstractMembers>
        {
            public int Compare(AbstractMembers x, AbstractMembers y)
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

                var abstractPropertyComparison = Comparer<AbstractNestedObject>.Default.Compare(x.AbstractProperty, y.AbstractProperty);
                if (abstractPropertyComparison != 0) {
                    return abstractPropertyComparison;
                }

                var interfacePropertyComparison = Comparer<INestedObject>.Default.Compare(x.InterfaceField, y.InterfaceField);
                if (interfacePropertyComparison != 0) {
                    return interfacePropertyComparison;
                }

                var compare = Comparer<BaseNestedObject>.Default.Compare(x.NotSealedProperty, y.NotSealedProperty);
                if (compare != 0) {
                    return compare;
                }

                if (x.ObjectField == null) {
                    if (y.ObjectField == null) {
                        return 0;
                    }

                    return -1;
                }

                if (y.ObjectField == null) {
                    return 1;
                }

                if (x.ObjectField is IComparable comparable
                    && y.ObjectField.GetType() == x.ObjectField.GetType()) {
                    return comparable.CompareTo(y.ObjectField);
                }

                throw new InvalidOperationException($"{nameof(ObjectField)} is not comparable.");
            }
        }
    }
}
