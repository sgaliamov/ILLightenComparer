using System.Collections.Generic;
using ILLightenComparer.Benchmarks.Models;
using Nito.Comparers;

namespace ILLightenComparer.Benchmarks.Benchmark.Comparer
{
    public class RegularModelComparerBenchmark : ComparersBenchmark<MovieModel>
    {
        private static readonly IComparer<MovieModel> Manual = MovieModelComparer.Instance;

        private static readonly IComparer<MovieModel> IlLightenComparer =
            new ComparerBuilder(c => c.SetDefaultCyclesDetection(false)
                                      .SetDefaultFieldsInclusion(false))
                .For<MovieModel>()
                .GetComparer();

        private static readonly IComparer<MovieModel> NitoComparer =
            Nito.Comparers.ComparerBuilder
                .For<MovieModel>()
                .OrderBy(x => x.Actors)
                .ThenBy(x => x.Genre)
                .ThenBy(x => x.Id)
                .ThenBy(x => x.Price)
                .ThenBy(x => x.ReleaseDate)
                .ThenBy(x => x.Title);

        public RegularModelComparerBenchmark() : base(Manual, IlLightenComparer, NitoComparer) { }
    }
}
