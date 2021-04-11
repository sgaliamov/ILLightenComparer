using System.Collections;
using System.Collections.Generic;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Comparers
{
    internal sealed class NullableComparer<TValue> : IComparer<TValue?>, IComparer
        where TValue : struct
    {
        private readonly IComparer<TValue> _valueComparer;

        public NullableComparer(IComparer<TValue> valueComparer = null)
        {
            _valueComparer = valueComparer ?? Helper.DefaultComparer<TValue>();
        }

        public int Compare(object x, object y)
        {
            if (ReferenceEquals(x, y)) {
                return 0;
            }

            if (y is null) {
                return 1;
            }

            if (x is null) {
                return -1;
            }

            return Compare((TValue)x, (TValue)y);
        }

        public int Compare(TValue? x, TValue? y)
        {
            if (!x.HasValue) {
                if (!y.HasValue) {
                    return 0;
                }

                return -1;
            }

            if (!y.HasValue) {
                return 1;
            }

            return _valueComparer.Compare(x.Value, y.Value);
        }
    }
}
