using System.Collections.Generic;
using System.Diagnostics;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.EqualityComparers
{
    [DebuggerDisplay("{_combinedHash64}")]
    internal sealed class HashCodeCombiner
    {
        public const long Seed = 0x1505L;

        public static HashCodeCombiner Start(long seed = Seed) => new(seed);

        public static HashCodeCombiner Combine<TItem>(params TItem[] objects) => Start().Combine(null, objects.UnfoldArrays());

        public static implicit operator int(HashCodeCombiner self) => self.CombinedHash;

        private long _combinedHash64;

        private HashCodeCombiner(long seed)
        {
            _combinedHash64 = seed;
        }

        public int CombinedHash => (int)_combinedHash64;

        public HashCodeCombiner Combine<TItem>(IEqualityComparer<TItem> itemComparer, TItem[] objects)
        {
            foreach (var o in objects) {
                var part = (_combinedHash64 << 5) + _combinedHash64;
                var hash = o is null ? 0 : itemComparer?.GetHashCode(o) ?? o?.GetHashCode() ?? 0;
                _combinedHash64 = part ^ hash;
            }

            return this;
        }

        public HashCodeCombiner CombineObjects<TItem>(params TItem[] objects) => Combine<TItem>(null, objects.UnfoldArrays());
    }
}
