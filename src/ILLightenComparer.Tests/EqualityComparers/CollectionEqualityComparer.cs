﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.EqualityComparers
{
    internal sealed class CollectionEqualityComparer<TItem> : IEqualityComparer<IEnumerable<TItem>>, IEqualityComparer, IHashSeedSetter
    {
        private readonly IEqualityComparer<TItem> _itemComparer;
        private readonly bool _sort;
        private long _seed = HashCodeCombiner.Seed;

        public CollectionEqualityComparer(IEqualityComparer<TItem> itemComparer = null, bool sort = false)
        {
            _sort = sort;
            _itemComparer = itemComparer ?? EqualityComparer<TItem>.Default;
        }

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
                Array.Sort(ax);
                x = ax;

                var ay = y.ToArray();
                Array.Sort(ay);
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

        bool IEqualityComparer.Equals(object x, object y) => Equals(x as IEnumerable<TItem>, y as IEnumerable<TItem>);

        public int GetHashCode(IEnumerable<TItem> obj) => HashCodeCombiner.Start(_seed).Combine<TItem>(_itemComparer, obj.ObjectToArray().Cast<TItem>().ToArray());

        public int GetHashCode(object obj) => GetHashCode(obj as IEnumerable<TItem>);

        public void SetHashSeed(long seed) => _seed = seed;
    }
}
