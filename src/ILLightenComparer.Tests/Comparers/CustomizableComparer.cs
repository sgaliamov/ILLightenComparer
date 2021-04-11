using System;
using System.Collections;
using System.Collections.Generic;

namespace ILLightenComparer.Tests.Comparers
{
    internal class CustomizableComparer<T> : IComparer<T>, IComparer
    {
        private readonly Func<T, T, int> _comparer;

        public CustomizableComparer(Func<T, T, int> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(object x, object y) => _comparer((T)x, (T)y);

        public int Compare(T x, T y) => _comparer(x, y);
    }
}
