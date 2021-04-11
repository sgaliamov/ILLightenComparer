using System.Collections.Generic;
using ILLightenComparer.Benchmarks.Models;
using Nito.Comparers;

namespace ILLightenComparer.Benchmarks.Benchmark.EqualityComparer
{
    public class RegularModelHashBenchmark : HashBenchmark<MovieModel>
    {
        private static readonly IEqualityComparer<MovieModel> Manual = MovieModelEqualityComparer.Instance;

        private static readonly IEqualityComparer<MovieModel> IlLightenComparer =
            new ComparerBuilder(c => c.SetDefaultCyclesDetection(false)
                                      .SetDefaultFieldsInclusion(false))
                .For<MovieModel>()
                .GetEqualityComparer();

        private static readonly IEqualityComparer<MovieModel> NitoComparer =
            Nito.Comparers.ComparerBuilder
                .For<MovieModel>()
                .OrderBy(x => x.Actors)
                .ThenBy(x => x.Genre)
                .ThenBy(x => x.Id)
                .ThenBy(x => x.Price)
                .ThenBy(x => x.ReleaseDate)
                .ThenBy(x => x.Title);

        public RegularModelHashBenchmark() : base(Manual, IlLightenComparer, NitoComparer) { }
    }
}
