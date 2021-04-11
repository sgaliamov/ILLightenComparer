using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ILLightenComparer.Tests.Comparers;

namespace ILLightenComparer.Benchmarks.Models
{
    public struct LightStruct
    {
        public byte Key;
        public char[] Value;
    }

    internal sealed class LightStructComparer : IComparer<LightStruct>
    {
        public static IComparer<LightStruct> Instance { get; } = new LightStructComparer();
        private readonly CollectionComparer<char> _collectionComparer = new();

        public int Compare(LightStruct x, LightStruct y)
        {
            var compare = x.Key.CompareTo(y.Key);
            if (compare != 0) {
                return compare;
            }

            return _collectionComparer.Compare(x.Value, y.Value);
        }
    }

    internal sealed class LightStructEqualityComparer : IEqualityComparer<LightStruct>
    {
        public static IEqualityComparer<LightStruct> Instance { get; } = new LightStructEqualityComparer();

        public bool Equals([AllowNull] LightStruct x, [AllowNull] LightStruct y) =>
            x.Key == y.Key
            && EqualityComparer<char[]>.Default.Equals(x.Value, y.Value);

        public int GetHashCode([DisallowNull] LightStruct obj)
        {
            var hash = 0x1505L;
            hash = ((hash << 5) + hash) ^ obj.Key;
            foreach (var item in obj.Value) {
                hash = ((hash << 5) + hash) ^ item;
            }

            return (int)hash;
        }
    }
}
