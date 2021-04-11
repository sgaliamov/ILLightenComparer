using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Samples
{
    public sealed class ComparableChildObject<TMember> :
        ComparableBaseObject<TMember>,
        IComparable<ComparableChildObject<TMember>>
    {
        [SuppressMessage("Design", "RCS1158:Static member in generic type should use a type parameter.", Justification = "Test class")]
        public new static bool UsedCompareTo;

        public static IComparer<TMember> ChildComparer = Helper.DefaultComparer<TMember>();

        public TMember ChildField;
        public TMember ChildProperty { get; set; }

        public int CompareTo(ComparableChildObject<TMember> other)
        {
            UsedCompareTo = true;
            ComparableBaseObject<TMember>.UsedCompareTo = true;

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

        public override int CompareTo(ComparableBaseObject<TMember> other) => CompareTo(other as ComparableChildObject<TMember>);

        public override string ToString() => this.ToJson();
    }
}
