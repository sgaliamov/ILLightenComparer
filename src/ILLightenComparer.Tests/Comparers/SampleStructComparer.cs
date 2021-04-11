using System.Collections;
using System.Collections.Generic;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Comparers
{
    internal sealed class SampleStructComparer<TMember> : IComparer<SampleStruct<TMember>>, IComparer
    {
        private readonly IComparer<TMember> _memberComparer;

        public SampleStructComparer(IComparer<TMember> memberComparer = null)
        {
            _memberComparer = memberComparer ?? Helper.DefaultComparer<TMember>();
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

            return Compare((SampleStruct<TMember>)x, (SampleStruct<TMember>)y);
        }

        public int Compare(SampleStruct<TMember> x, SampleStruct<TMember> y)
        {
            var compare = _memberComparer.Compare(x.Field, y.Field);
            if (compare != 0) {
                return compare;
            }

            return _memberComparer.Compare(x.Property, y.Property);
        }
    }
}
