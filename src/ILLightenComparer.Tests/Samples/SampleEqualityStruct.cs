using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Samples
{
    [SuppressMessage("Usage", "CA2231:Overload operator equals on overriding value type Equals", Justification = "Test class")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Test class")]
    [SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Test class")]
    public struct SampleEqualityStruct<TMember> : IComparable<SampleEqualityStruct<TMember>>
    {
        private static readonly IComparer<TMember> Comparer = Helper.DefaultComparer<TMember>();
        public static IEqualityComparer<TMember> EqualityComparer = EqualityComparer<TMember>.Default;

        public TMember Field;
        public TMember Property { get; set; }

        public override string ToString() => $"Struct: {this.ToJson()}";

        public override bool Equals(object obj) => Equals((SampleEqualityStruct<TMember>)obj);

        public bool Equals(SampleEqualityStruct<TMember> other) =>
            EqualityComparer.Equals(Field, other.Field)
            && EqualityComparer.Equals(Property, other.Property);

        public override int GetHashCode() => HashCodeCombiner.Combine(Field, Property);

        public int CompareTo(SampleEqualityStruct<TMember> other)
        {
            var compare = Comparer.Compare(Field, other.Field);
            if (compare != 0) {
                return compare;
            }

            return Comparer.Compare(Property, other.Property);
        }
    }
}
