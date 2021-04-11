using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;

namespace ILLightenComparer.Tests.ComparerTests
{
    internal static class TestTypes
    {
        static TestTypes()
        {
            Types = new Dictionary<Type, IComparer> {
                [typeof(object)] = null,
                [typeof(sbyte)] = null,
                [typeof(byte)] = null,
                [typeof(char)] = null,
                [typeof(short)] = null,
                [typeof(ushort)] = null,
                [typeof(int)] = null,
                [typeof(long)] = null,
                [typeof(ulong)] = null,
                [typeof(float)] = null,
                [typeof(double)] = null,
                [typeof(decimal)] = null,
                [typeof(EnumSmall)] = null,
                [typeof(EnumBig)] = null,
                [typeof(string)] = null,
                [typeof(DateTime)] = null,
                [typeof(DateTimeOffset)] = null,
                [typeof(Guid)] = null,
                [typeof(TimeSpan)] = null,
                [typeof(ComparableBaseObject<EnumSmall?>)] = null,
                [typeof(ComparableChildObject<EnumSmall?>)] = null,
                [typeof(ComparableStruct<EnumSmall?>)] = null,
                [typeof(SampleObject<EnumSmall?>)] = new SampleObjectComparer<EnumSmall?>(),
                [typeof(SampleStruct<EnumSmall?>)] = new SampleStructComparer<EnumSmall?>()
            };

            NullableTypes = Types
                            .Where(x => x.Key.IsValueType)
                            .ToDictionary(x => x.Key.MakeNullable(), x => Helper.CreateNullableComparer(x.Key, x.Value));
        }

        public static IDictionary<Type, IComparer> NullableTypes { get; }

        public static IDictionary<Type, IComparer> Types { get; }
    }
}
