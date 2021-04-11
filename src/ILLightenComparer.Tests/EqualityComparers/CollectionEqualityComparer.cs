using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.EqualityComparers
{
    internal sealed class CollectionEqualityComparer<TItem> : IEqualityComparer<IEnumerable<TItem>>, IEqualityComparer, IHashSeedSetter
    {
        private readonly IEqualityComparer<TItem> _itemComparer;
        private readonly AsyncLocal<long> _seed = new();
        private readonly bool _sort;
        private readonly IComparer<TItem> _sortComparer;

        public CollectionEqualityComparer(IEqualityComparer<TItem> itemComparer = null, bool sort = false, IComparer<TItem> sortComparer = null)
        {
            _sort = sort;
            _sortComparer = sortComparer ?? Helper.DefaultComparer<TItem>();
            _itemComparer = itemComparer ?? EqualityComparer<TItem>.Default;
            _seed.Value = HashCodeCombiner.Seed;
        }

        bool IEqualityComparer.Equals(object x, object y) => Equals((IEnumerable<TItem>)x, (IEnumerable<TItem>)y);

        public bool Equals(IEnumerable<TItem> x, IEnumerable<TItem> y)
        {
            if (x == null) {
                return y == null;
            }

            if (y == null) {
                return false;
            }

            if (_sort) {
                var ax = x.ToArray();
                Array.Sort(ax, _sortComparer);
                x = ax;

                var ay = y.ToArray();
                Array.Sort(ay, _sortComparer);
                y = ay;
            }

            using var enumeratorX = x.GetEnumerator();
            using var enumeratorY = y.GetEnumerator();

            while (true) {
                var xDone = !enumeratorX.MoveNext();
                var yDone = !enumeratorY.MoveNext();

                if (xDone) {
                    return yDone;
                }

                if (yDone) {
                    return false;
                }

                var xCurrent = enumeratorX.Current;
                var yCurrent = enumeratorY.Current;

                var compare = _itemComparer.Equals(xCurrent, yCurrent);
                if (!compare) {
                    return false;
                }
            }
        }

        public int GetHashCode(object obj) => GetHashCode((IEnumerable<TItem>)obj);

        public int GetHashCode(IEnumerable<TItem> obj)
        {
            if (obj is null) {
                return 0;
            }

            if (_sort) {
                var array = obj.ToArray();
                Array.Sort(array, _sortComparer);
                obj = array;
            }

            return HashCodeCombiner.Start(_seed.Value).Combine(_itemComparer, obj.ToArray());
        }

        public void SetHashSeed(long seed) => _seed.Value = seed;
    }
}
