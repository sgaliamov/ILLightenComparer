using System.Collections.Generic;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.ComparerTests.CycleTests.Samples
{
    public sealed class CycledStructObject
    {
        public static IComparer<CycledStructObject> Comparer { get; } = new RelationalComparer();
        public readonly int Id;
        public CycledStruct? FirstStruct;
        public string TextField;

        public CycledStructObject()
        {
            Id = this.GetObjectId();
        }

        public CycledStruct SecondStruct { get; set; }

        public override string ToString() => Id.ToString();

        public sealed class RelationalComparer : IComparer<CycledStructObject>
        {
            public static int Compare(
                CycledStructObject x,
                CycledStructObject y,
                CycleDetectionSet setX,
                CycleDetectionSet setY)
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

                // & because, both methods need to be executed.
                if (!setX.TryAdd(x, 0) & !setY.TryAdd(y, 0)) {
                    return setX.Count - setY.Count;
                }

                var compare = string.CompareOrdinal(x.TextField, y.TextField);
                if (compare != 0) {
                    return compare;
                }

                compare = CycledStruct.RelationalComparer.Compare(x.FirstStruct, y.FirstStruct, setX, setY);
                if (compare != 0) {
                    return compare;
                }

                return CycledStruct.RelationalComparer.Compare(x.SecondStruct, y.SecondStruct, setX, setY);
            }

            public int Compare(CycledStructObject x, CycledStructObject y)
            {
                var setX = new CycleDetectionSet();
                var setY = new CycleDetectionSet();

                return Compare(x, y, setX, setY);
            }
        }
    }
}
