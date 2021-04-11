using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Samples
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Test class")]
    [SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Test class")]
    public struct ComparableStruct<TMember> : IComparable<ComparableStruct<TMember>>
    {
        [SuppressMessage("Design", "RCS1158:Static member in generic type should use a type parameter.", Justification = "Test class")]
        public static bool UsedCompareTo;

        public static IComparer<TMember> Comparer = Helper.DefaultComparer<TMember>();

        public TMember Field;
        public TMember Property { get; set; }

        public int CompareTo(ComparableStruct<TMember> other)
        {
            UsedCompareTo = true;

            var compare = Comparer.Compare(Field, other.Field);
            if (compare != 0) {
                return compare;
            }

            return Comparer.Compare(Property, other.Property);
        }

        public override string ToString() => this.ToJson();
    }
}
