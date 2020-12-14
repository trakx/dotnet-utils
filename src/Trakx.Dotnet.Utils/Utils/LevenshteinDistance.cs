using System;
using System.Collections.Generic;
using System.Linq;

namespace Trakx.Dotnet.Utils.Utils
{
    /// <summary>
    /// Contains approximate string matching utils
    /// </summary>
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the Levenshtein distance distance between two strings, with case sensitive scoring.
        /// From https://stackoverflow.com/questions/13793560/find-closest-match-to-input-string-in-a-list-of-strings/13793600
        /// </summary>
        public static decimal CalculateLevenshteinDistance(this string source, string target)
        {
            var n = source.Length;
            var m = target.Length;
            var d = new decimal[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (var i = 0; i <= n; d[i, 0] = i++) { }

            for (var j = 0; j <= m; d[0, j] = j++) { }

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1]) 
                        ? 0 
                        : char.ToUpperInvariant(target[j - 1]) == char.ToUpperInvariant(source[i - 1])
                            ? 0.5m : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }


        public static string? FindBestLevenshteinMatch(this string source,
            IEnumerable<string> candidateList,
            decimal maxDistance = decimal.MaxValue)
        {
            var orderedCandidates = candidateList.Select(c => new
            {
                Value = c,
                Distance = CalculateLevenshteinDistance(source, c)
            }).OrderBy(c => c.Distance);

            return orderedCandidates
                .FirstOrDefault(c => c.Distance <= maxDistance)?.Value;
        }
    }

}
