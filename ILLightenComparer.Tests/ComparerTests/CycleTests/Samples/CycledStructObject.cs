﻿using System;
using System.Collections.Generic;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.ComparerTests.CycleTests.Samples
{
    public sealed class CycledStructObject
    {
        public readonly int Id;
        public CycledStruct? FirstStruct;
        public string TextField;

        public CycledStructObject()
        {
            Id = this.GetObjectId();
        }

        public static IComparer<CycledStructObject> Comparer { get; } = new RelationalComparer();

        public CycledStruct SecondStruct { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }

        public sealed class RelationalComparer : IComparer<CycledStructObject>
        {
            public int Compare(CycledStructObject x, CycledStructObject y)
            {
                var setX = new ConcurrentSet<object>();
                var setY = new ConcurrentSet<object>();

                return Compare(x, y, setX, setY);
            }

            public static int Compare(
                CycledStructObject x,
                CycledStructObject y,
                ConcurrentSet<object> setX,
                ConcurrentSet<object> setY)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (ReferenceEquals(null, y))
                {
                    return 1;
                }

                if (ReferenceEquals(null, x))
                {
                    return -1;
                }

                if (!setX.TryAdd(x, 0) & !setY.TryAdd(y, 0))
                {
                    return setX.Count - setY.Count;
                }

                var compare = string.Compare(x.TextField, y.TextField, StringComparison.Ordinal);
                if (compare != 0)
                {
                    return compare;
                }

                compare = CycledStruct.RelationalComparer.Compare(x.FirstStruct, y.FirstStruct, setX, setY);
                if (compare != 0)
                {
                    return compare;
                }

                return CycledStruct.RelationalComparer.Compare(x.SecondStruct, y.SecondStruct, setX, setY);
            }
        }
    }
}
