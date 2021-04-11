using System;
using ILLightenComparer.Tests.Samples;

namespace ILLightenComparer.Tests.ComparerTests.HierarchyTests.Samples.Nested
{
    public class BaseNestedObject : AbstractNestedObject
    {
        public EnumSmall? Key { get; set; }

        public override int CompareTo(object obj)
        {
            if (obj is null) {
                return 1;
            }

            if (ReferenceEquals(this, obj)) {
                return 0;
            }

            return obj is BaseNestedObject other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(BaseNestedObject)}");
        }

        private int CompareTo(BaseNestedObject other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            var result = Nullable.Compare(Key, other.Key);
            if (result != 0) {
                return result;
            }

            return base.CompareTo(other);
        }
    }
}
