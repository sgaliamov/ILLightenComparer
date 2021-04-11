using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Samples
{
    [SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Test class")]
    public sealed class ComparableObject<TMember> : IComparable<ComparableObject<TMember>>
    {
        private static readonly IComparer<TMember> Comparer = Helper.DefaultComparer<TMember>();

        public TMember Field;
        public TMember Property { get; set; }

        public int CompareTo(ComparableObject<TMember> other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            var compare = Comparer.Compare(Field, other.Field);
            if (compare != 0) {
                return compare;
            }

            return Comparer.Compare(Property, other.Property);
        }

        public override string ToString() => $"Object: {this.ToJson()}";
    }
}
