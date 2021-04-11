using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ILLightenComparer.Benchmarks.Models
{
    public sealed class MovieModel
    {
        public string[] Actors { get; set; }
        public string Genre { get; set; }
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Title { get; set; }
    }

    internal sealed class MovieModelComparer : IComparer<MovieModel>
    {
        public static IComparer<MovieModel> Instance { get; } = new MovieModelComparer();

        public int Compare(MovieModel x, MovieModel y)
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

            if (x.Actors != null) {
                var actorsComparison = ActorsCollectionComparer.Instance.Compare(x.Actors, y.Actors);
                if (actorsComparison != 0) {
                    return actorsComparison;
                }
            } else if (y.Actors != null) {
                return -1;
            }

            var genreComparison = string.CompareOrdinal(x.Genre, y.Genre);
            if (genreComparison != 0) {
                return genreComparison;
            }

            var idComparison = x.Id.CompareTo(y.Id);
            if (idComparison != 0) {
                return idComparison;
            }

            var priceComparison = x.Price.CompareTo(y.Price);
            if (priceComparison != 0) {
                return priceComparison;
            }

            var releaseDateComparison = x.ReleaseDate.CompareTo(y.ReleaseDate);
            if (releaseDateComparison != 0) {
                return releaseDateComparison;
            }

            return string.CompareOrdinal(x.Title, y.Title);
        }
    }

    internal sealed class MovieModelEqualityComparer : IEqualityComparer<MovieModel>
    {
        public static IEqualityComparer<MovieModel> Instance { get; } = new MovieModelEqualityComparer();

        public bool Equals([AllowNull] MovieModel one, [AllowNull] MovieModel other) =>
            one == other
            || one != null
            && other != null
            && EqualityComparer<string[]>.Default.Equals(one.Actors, other.Actors)
            && string.Equals(one.Genre, other.Genre, StringComparison.Ordinal)
            && one.Id == other.Id
            && one.Price == other.Price
            && one.ReleaseDate == other.ReleaseDate
            && string.Equals(one.Title, other.Title, StringComparison.Ordinal);

        public int GetHashCode([DisallowNull] MovieModel obj)
        {
            var hash = 0x1505L;
            hash = ((hash << 5) + hash) ^ EqualityComparer<string[]>.Default.GetHashCode(obj.Actors);
            hash = ((hash << 5) + hash) ^ obj.Genre.GetHashCode();
            hash = ((hash << 5) + hash) ^ obj.Id.GetHashCode();
            hash = ((hash << 5) + hash) ^ obj.Price.GetHashCode();
            hash = ((hash << 5) + hash) ^ obj.ReleaseDate.GetHashCode();
            hash = ((hash << 5) + hash) ^ obj.Title.GetHashCode();

            return (int)hash;
        }
    }

    internal sealed class ActorsCollectionComparer : IComparer<string[]>
    {
        public static IComparer<string[]> Instance { get; } = new ActorsCollectionComparer();

        public int Compare([AllowNull] string[] x, [AllowNull] string[] y)
        {
            if (x == y) {
                return 0;
            }

            if (x is null) {
                return -1;
            }

            if (y is null) {
                return 1;
            }

            for (var i = 0;; i++) {
                if (i == x.Length) {
                    return i == y.Length ? 0 : 1;
                }

                if (i == y.Length) {
                    return -1;
                }

                var c = string.Compare(x[i], y[i], StringComparison.Ordinal);
                if (c != 0) {
                    return c;
                }
            }
        }
    }
}
