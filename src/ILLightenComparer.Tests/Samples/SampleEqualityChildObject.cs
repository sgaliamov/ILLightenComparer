using System;
using System.Collections.Generic;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Samples
{
    public sealed class SampleEqualityChildObject<TMember> : SampleEqualityBaseObject<TMember>, IComparable<SampleEqualityChildObject<TMember>>
    {
        private static readonly IComparer<TMember> ChildComparer = Helper.DefaultComparer<TMember>();
        public static IEqualityComparer<TMember> ChildEqualityComparer = EqualityComparer<TMember>.Default;

        public TMember ChildField;
        public TMember ChildProperty { get; set; }

        public int CompareTo(SampleEqualityChildObject<TMember> other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            var compare = base.CompareTo(other);
            if (compare != 0) {
                return compare;
            }

            compare = ChildComparer.Compare(ChildField, other.ChildField);
            if (compare != 0) {
                return compare;
            }

            return ChildComparer.Compare(ChildProperty, other.ChildProperty);
        }

        public override int CompareTo(SampleEqualityBaseObject<TMember> other) => CompareTo((SampleEqualityChildObject<TMember>)other);

        public override bool Equals(object obj) => Equals((SampleEqualityChildObject<TMember>)obj);

        public bool Equals(SampleEqualityChildObject<TMember> other) =>
            other != null
            && base.Equals(other)
            && ChildEqualityComparer.Equals(ChildField, other.ChildField)
            && ChildEqualityComparer.Equals(ChildProperty, other.ChildProperty);

        public override int GetHashCode() => HashCodeCombiner.Combine(Field, Property, ChildField, ChildProperty);

        public override string ToString() => this.ToJson();
    }
}
