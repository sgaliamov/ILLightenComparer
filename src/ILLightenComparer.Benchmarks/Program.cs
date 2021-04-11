using BenchmarkDotNet.Running;
using ILLightenComparer.Benchmarks.Benchmark;
using ILLightenComparer.Benchmarks.Benchmark.Comparer;
using ILLightenComparer.Benchmarks.Benchmark.EqualityComparer;

namespace ILLightenComparer.Benchmarks
{
    public sealed class Program
    {
        /// <summary>
        ///     Benchmarks runner for ILLightenComparer.
        /// </summary>
        /// <param name="compare">Runs benchmarks for IComparer&lt;T&gt;.</param>
        /// <param name="equals">Runs benchmarks for IEqualityComparer&lt;T&gt;.Equals.</param>
        /// <param name="hash">Runs benchmarks for IEqualityComparer&lt;T&gt;.GetHashCode.</param>
        /// <param name="struct">Runs benchmarks for a class.</param>
        /// <param name="class">Runs benchmarks for a struct.</param>
        /// <param name="misc">Runs others benchmarks.</param>
        public static void Main(
            bool compare = false,
            bool equals = false,
            bool hash = false,
            bool @struct = false,
            bool @class = false,
            bool misc = false)
        {
            if (compare && @struct) {
                BenchmarkRunner.Run<LightStructComparerBenchmark>();
            }

            if (compare && @class) {
                BenchmarkRunner.Run<RegularModelComparerBenchmark>();
            }

            if (equals && @struct) {
                BenchmarkRunner.Run<LightStructEqualityBenchmark>();
            }

            if (equals && @class) {
                BenchmarkRunner.Run<RegularModelEqualityBenchmark>();
            }

            if (hash && @struct) {
                BenchmarkRunner.Run<LightStructHashBenchmark>();
            }

            if (hash && @class) {
                BenchmarkRunner.Run<RegularModelHashBenchmark>();
            }

            if (misc) {
                BenchmarkRunner.Run<CompareIntegral>();
                BenchmarkRunner.Run<EqualityBenchmark>();
            }
        }
    }
}
