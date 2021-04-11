using System.Collections;
using System.Collections.Generic;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.Comparers
{
    internal sealed class SampleObjectComparer<TMember> : IComparer<SampleObject<TMember>>, IComparer
    {
        private readonly IComparer<TMember> _memberComparer;

        public SampleObjectComparer(IComparer<TMember> memberComparer = null)
        {
            _memberComparer = memberComparer ?? Helper.DefaultComparer<TMember>();
        }

        public int Compare(object x, object y) => Compare(x as SampleObject<TMember>, y as SampleObject<TMember>);

        public int Compare(SampleObject<TMember> x, SampleObject<TMember> y)
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

            var compare = _memberComparer.Compare(x.Field, y.Field);
            if (compare != 0) {
                return compare;
            }

            return _memberComparer.Compare(x.Property, y.Property);
        }
    }
}
