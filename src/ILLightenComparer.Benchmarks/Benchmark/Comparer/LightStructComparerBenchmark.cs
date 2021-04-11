using System.Collections.Generic;
using ILLightenComparer.Benchmarks.Models;
using Nito.Comparers;

namespace ILLightenComparer.Benchmarks.Benchmark.Comparer
{
    public class LightStructComparerBenchmark : ComparersBenchmark<LightStruct>
    {
        private static readonly IComparer<LightStruct> Native = LightStructComparer.Instance;

        private static readonly IComparer<LightStruct> ILLightenComparer =
            new ComparerBuilder(c => c.SetDefaultCyclesDetection(false))
                .For<LightStruct>()
                .GetComparer();

        private static readonly IComparer<LightStruct> NitoComparer =
            Nito.Comparers.ComparerBuilder
                .For<LightStruct>()
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Value);

        public LightStructComparerBenchmark() : base(Native, ILLightenComparer, NitoComparer) { }
    }
}
