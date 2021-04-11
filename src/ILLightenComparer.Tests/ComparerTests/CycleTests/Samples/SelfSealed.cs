using System.Collections.Generic;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.ComparerTests.CycleTests.Samples
{
    public sealed class SelfSealed
    {
        public static RelationalComparer Comparer { get; } = new();
        public readonly int Id;
        public SelfSealed First;

        public SelfSealed()
        {
            Id = this.GetObjectId();
        }

        public SelfSealed Second { get; set; }
        public int Value { get; set; }

        public bool Equals(SelfSealed obj) => Value == obj.Value;

        public override bool Equals(object obj) => Equals((SelfSealed)obj);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Id.ToString();

        public sealed class RelationalComparer : IComparer<SelfSealed>
        {
            private static int Compare(SelfSealed x, SelfSealed y, CycleDetectionSet setX, CycleDetectionSet setY)
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

                var compareFirst = Compare(x.First, y.First, setX, setY);
                if (compareFirst != 0) {
                    return compareFirst;
                }

                var compareSecond = Compare(x.Second, y.Second, setX, setY);
                if (compareSecond != 0) {
                    return compareSecond;
                }

                var compareValue = x.Value.CompareTo(y.Value);
                if (compareValue != 0) {
                    return compareValue;
                }

                setX.Remove(x, out _);
                setY.Remove(y, out _);

                return 0;
            }

            public int Compare(SelfSealed x, SelfSealed y)
            {
                var setX = new CycleDetectionSet();
                var setY = new CycleDetectionSet();

                return Compare(x, y, setX, setY);
            }

            public int Compare(IEnumerable<SelfSealed> x, IEnumerable<SelfSealed> y)
            {
                var setX = new CycleDetectionSet();
                var setY = new CycleDetectionSet();

                using var enumeratorX = x.GetEnumerator();
                using var enumeratorY = y.GetEnumerator();

                while (true) {
                    var xDone = !enumeratorX.MoveNext();
                    var yDone = !enumeratorY.MoveNext();

                    if (xDone) {
                        return yDone ? 0 : -1;
                    }

                    if (yDone) {
                        return 1;
                    }

                    var xCurrent = enumeratorX.Current;
                    var yCurrent = enumeratorY.Current;

                    var compare = Compare(xCurrent, yCurrent, setX, setY);
                    if (compare != 0) {
                        return compare;
                    }
                }
            }
        }
    }
}
