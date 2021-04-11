using System;
using System.Linq;
using System.Reflection;
using AutoFixture.Kernel;

namespace ILLightenComparer.Tests.Utilities
{
    internal sealed class CustomNumericGenerator : ISpecimenBuilder
    {
        private readonly int _lower;
        private readonly double _minMaxProbability;
        private readonly int _upper;

        public CustomNumericGenerator(int lower, int upper, double minMaxProbability)
        {
            _lower = lower;
            _upper = upper;
            _minMaxProbability = minMaxProbability;
        }

        public object Create(object request, ISpecimenContext context) => request is Type type
            ? type.GetTypeInfo().IsEnum
                ? CreateRandomEnum(type)
                : CreateRandom(type)
            : new NoSpecimen();

        private object CreateRandom(Type type) => Type.GetTypeCode(type) switch {
            TypeCode.Byte => MinMax<byte>(type) ?? (byte)GetNextRandom(),
            TypeCode.Decimal => MinMax<decimal>(type) ?? GetNextRandom(),
            TypeCode.Double => MinMax<double>(type) ?? GetNextRandom(),
            TypeCode.Int16 => MinMax<short>(type) ?? (short)GetNextRandom(),
            TypeCode.Int32 => MinMax<int>(type) ?? (int)GetNextRandom(),
            TypeCode.Int64 => MinMax<long>(type) ?? GetNextRandom(),
            TypeCode.SByte => MinMax<sbyte>(type) ?? (sbyte)GetNextRandom(),
            TypeCode.Single => MinMax<float>(type) ?? GetNextRandom(),
            TypeCode.UInt16 => MinMax<ushort>(type) ?? (ushort)GetNextRandom(),
            TypeCode.UInt32 => MinMax<uint>(type) ?? (uint)GetNextRandom(),
            TypeCode.UInt64 => MinMax<ulong>(type) ?? (ulong)GetNextRandom(),
            _ => new NoSpecimen()
        };

        private object CreateRandomEnum(Type type)
        {
            var values = Enum.GetValues(type).Cast<object>().OrderBy(x => x).ToArray();

            var index = ThreadSafeRandom.NextDouble() < _minMaxProbability
                ? ThreadSafeRandom.NextDouble() < 0.5 ? 0 : values.Length - 1
                : ThreadSafeRandom.Next(0, values.Length);

            return values[index];
        }

        private long GetNextRandom() => ThreadSafeRandom.Next(_lower, _upper);

        private T? MinMax<T>(IReflect request) where T : struct
        {
            if (ThreadSafeRandom.NextDouble() >= _minMaxProbability) {
                return null;
            }

            object Get(IReflect t, string name) => t.GetField(name, BindingFlags.Static | BindingFlags.Public).GetValue(null);

            return ThreadSafeRandom.NextDouble() < 0.5
                ? (T)Get(request, "MinValue")
                : (T)Get(request, "MaxValue");
        }
    }
}
