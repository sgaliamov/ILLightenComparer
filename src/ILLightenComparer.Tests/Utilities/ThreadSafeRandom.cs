using System;

namespace ILLightenComparer.Tests.Utilities
{
    internal sealed class ThreadSafeRandom
    {
        private static readonly Random Global = new();

        [ThreadStatic] private static Random _local;

        public static double NextDouble() => GetInstance().NextDouble();

        public static int Next(int minValue, int maxValue) => GetInstance().Next(minValue, maxValue);

        private static Random GetInstance() => _local ?? (_local = new Random(GetSeed()));

        private static int GetSeed()
        {
            lock (Global) {
                return Global.Next();
            }
        }
    }
}
