using System.Collections;
using System.Collections.Generic;
using ILLightenComparer.Tests.Samples;

namespace ILLightenComparer.Tests.EqualityComparers
{
    internal sealed class ComparableStructEqualityComparer<TMember> : IEqualityComparer<ComparableStruct<TMember>>, IEqualityComparer
    {
        private readonly IEqualityComparer _memberComparer;

        public ComparableStructEqualityComparer(IEqualityComparer memberComparer = null)
        {
            _memberComparer = memberComparer ?? EqualityComparer<TMember>.Default;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (ReferenceEquals(x, y)) {
                return true;
            }

            if (y is null || x is null) {
                return false;
            }

            return Equals((ComparableStruct<TMember>)x, (ComparableStruct<TMember>)y);
        }

        public bool Equals(ComparableStruct<TMember> x, ComparableStruct<TMember> y)
        {
            var compare = _memberComparer.Equals(x.Field, y.Field);
            if (!compare) {
                return false;
            }

            return _memberComparer.Equals(x.Property, y.Property);
        }

        public int GetHashCode(object obj) => GetHashCode((ComparableStruct<TMember>)obj);

        public int GetHashCode(ComparableStruct<TMember> obj)
        {
            var setter = _memberComparer as IHashSeedSetter;
            var combiner = HashCodeCombiner.Start();

            setter?.SetHashSeed(combiner.CombinedHash);
            combiner.CombineObjects(obj.Field is null ? 0 : _memberComparer.GetHashCode(obj.Field));

            setter?.SetHashSeed(combiner.CombinedHash);
            return combiner.CombineObjects(obj.Property is null ? 0 : _memberComparer.GetHashCode(obj.Property));
        }
    }
}
