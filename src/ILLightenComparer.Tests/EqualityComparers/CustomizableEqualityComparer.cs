using System;
using System.Collections;
using System.Collections.Generic;

namespace ILLightenComparer.Tests.EqualityComparers
{
    internal class CustomizableEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        private readonly Func<T, T, bool> _comparer;
        private readonly Func<T, int> _hasher;

        public CustomizableEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hasher)
        {
            _comparer = comparer;
            _hasher = hasher;
        }

        public new bool Equals(object x, object y) => Equals((T)x, (T)y);

        public bool Equals(T x, T y) => _comparer(x, y);

        public int GetHashCode(object obj) => GetHashCode((T)obj);

        public int GetHashCode(T obj) => _hasher(obj);
    }
}
