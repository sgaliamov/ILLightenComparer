using System.Collections;
using System.Collections.Generic;
using ILLightenComparer.Tests.Samples;

namespace ILLightenComparer.Tests.EqualityComparers
{
    internal sealed class ComparableObjectEqualityComparer<TMember> : IEqualityComparer<ComparableObject<TMember>>, IEqualityComparer
    {
        private readonly IEqualityComparer _memberComparer;

        public ComparableObjectEqualityComparer(IEqualityComparer memberComparer = null)
        {
            _memberComparer = memberComparer ?? EqualityComparer<TMember>.Default;
        }

        bool IEqualityComparer.Equals(object x, object y) => Equals((ComparableObject<TMember>)x, (ComparableObject<TMember>)y);

        public bool Equals(ComparableObject<TMember> x, ComparableObject<TMember> y)
        {
            if (ReferenceEquals(x, y)) {
                return true;
            }

            if (y is null || x is null) {
                return false;
            }

            var compare = _memberComparer.Equals(x.Field, y.Field);
            if (!compare) {
                return false;
            }

            return _memberComparer.Equals(x.Property, y.Property);
        }

        public int GetHashCode(object obj) => GetHashCode((ComparableObject<TMember>)obj);

        public int GetHashCode(ComparableObject<TMember> obj)
        {
            if (obj is null) {
                return 0;
            }

            var setter = _memberComparer as IHashSeedSetter;
            var combiner = HashCodeCombiner.Start();

            setter?.SetHashSeed(combiner.CombinedHash);
            combiner.CombineObjects(obj.Field is null ? 0 : _memberComparer.GetHashCode(obj.Field));

            setter?.SetHashSeed(combiner.CombinedHash);
            return combiner.CombineObjects(obj.Property is null ? 0 : _memberComparer.GetHashCode(obj.Property));
        }
    }
}
