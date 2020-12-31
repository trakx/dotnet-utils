﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Trakx.Utils.Extensions
{
    public static class StringExtensions
    {
        public static Regex EthereumAddressRegex { get; } =
            new Regex(@"^(?<Prefix>0x)(?<Address>[A-F,a-f,0-9]{40})$");

        public static string UrlEncode(this string rawString)
        {
            return UrlEncoder.Default.Encode(rawString);
        }

        public static bool IsValidEthereumAddress(this string address)
        {
            return EthereumAddressRegex.IsMatch(address);
        }

        public static string ToHexString(this byte[] array)
        {
            return BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
        }

        public static List<string> SplitCsvToLowerCaseDistinctList(this string csvString)
        {
            if (string.IsNullOrWhiteSpace(csvString)) return new List<string>();
            var values = csvString
                .Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.ToLowerInvariant());
            return values.Distinct().ToList();
        }

        #region Levenshtein distance

        /// <summary>
        /// Compute the Levenshtein distance distance between two strings, with case sensitive scoring.
        /// From https://stackoverflow.com/questions/13793560/find-closest-match-to-input-string-in-a-list-of-strings/13793600
        /// </summary>
        public static decimal CalculateLevenshteinDistance(this string source, string target)
        {
            var n = source.Length;
            var m = target.Length;
            var d = new decimal[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (var i = 0; i <= n; d[i, 0] = i++) { /**/ }
            for (var j = 0; j <= m; d[0, j] = j++) { /**/ }
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {

#pragma warning disable S3358 // Ternary operators should not be nested
                    var cost = target[j - 1] == source[i - 1]
                        ? 0
                        : char.ToUpperInvariant(target[j - 1]) == char.ToUpperInvariant(source[i - 1])
                            ? 0.5m : 1;
#pragma warning restore S3358 // Ternary operators should not be nested

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

        #endregion
    }
}
