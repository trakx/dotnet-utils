using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace Trakx.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> IntersectMany<T>(this IEnumerable<IEnumerable<T>> enumOfEnums)
        {
            var ofEnums = enumOfEnums as List<IEnumerable<T>> ?? enumOfEnums.ToList();
            if (!ofEnums.Any()) return Enumerable.Empty<T>();
            var intersection = ofEnums
                .Skip(1)
                .Aggregate(
                    new HashSet<T>(ofEnums.First()),
                    (h, e) =>
                    {
                        h.IntersectWith(e);
                        return h;
                    }
                );
            return intersection;
        }

        /// <summary>
        /// Partitions the given list around a pivot element such that all elements on left of pivot are <= pivot
        /// and the ones at thr right are > pivot. This method can be used for sorting, N-order statistics such as
        /// as median finding algorithms.
        /// Pivot is selected randomly if random number generator is supplied else its selected as last element in the list.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 171
        /// </summary>
        private static int Partition<T>(this IList<T> list, int start, int end, Random? rnd = null)
            where T : IComparable<T>
        {
            if (rnd != null)
                list.Swap(end, rnd.Next(start, end + 1));

            var pivot = list[end];
            var lastLow = start - 1;
            for (var i = start; i < end; i++)
            {
                if (list[i].CompareTo(pivot) <= 0)
                    list.Swap(i, ++lastLow);
            }

            list.Swap(end, ++lastLow);
            return lastLow;
        }

        /// <summary>
        /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
        /// Note: specified list would be mutated in the process.
        /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 216
        /// </summary>
        public static T NthOrderStatistic<T>(this IList<T> list, int n, Random? rnd = null) where T : IComparable<T>
        {
            return NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
        }

        private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random? rnd)
            where T : IComparable<T>
        {
            while (true)
            {
                var pivotIndex = list.Partition(start, end, rnd);
                if (pivotIndex == n)
                    return list[pivotIndex];

                if (n < pivotIndex)
                    end = pivotIndex - 1;
                else
                    start = pivotIndex + 1;
            }
        }

        // Credits to https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            if (i == j) //This check is not required but Partition function may make many calls so its for perf reason
                return;
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        /// <summary>
        /// Note: specified list would be mutated in the process.
        /// </summary>
        public static T Median<T>(this IList<T> list) where T : IComparable<T>
        {
            return list.NthOrderStatistic((list.Count - 1) / 2);
        }

        public static double Median<T>(this IEnumerable<T> sequence, Func<T, double> getValue)
        {
            var list = sequence.Select(getValue).ToList();
            var mid = (list.Count - 1) / 2;
            return list.NthOrderStatistic(mid);
        }

        public static decimal Median<T>(this IEnumerable<T> sequence, Func<T, decimal> getValue)
        {
            var list = sequence.Select(getValue).ToList();
            var mid = (list.Count - 1) / 2;
            return list.NthOrderStatistic(mid);
        }

        public readonly struct SelectionWithMeanStandardDeviation<T>
        {
            public SelectionWithMeanStandardDeviation(T? selection, double mean, double standardDeviation)
            {
                Selection = selection;
                Mean = mean;
                StandardDeviation = standardDeviation;
            }

            public readonly T? Selection;
            public readonly double Mean;
            public readonly double StandardDeviation;
        }

        /// <summary>
        /// Returns the first value, in order of preference, of a given list of <see cref="T"/>, where <see cref="T"/>
        /// are measured by the value returned by the <see cref="valueSelector"/>.
        /// Useful for choosing a price from a list of quotes by different market data providers, one of which can
        /// return an erroneous quote.
        /// If no match is found below <see cref="maxStandardDeviations"/>, the one with the lowest standard deviation is returned.
        /// </summary>
        /// <typeparam name="T">Type of the items that are being sorter by preference.</typeparam>
        /// <param name="preferences">List of items to choose from, ordered by preference.</param>
        /// <param name="valueSelector">Function used to select the value on which to base the statistics used to pick a value.</param>
        /// <param name="maxStandardDeviations">Maximum number of standard deviation on which to base the selection. Beyond that value, a
        /// preferred value is rejected and the next preferred one will be evaluated.</param>
        /// <param name="throwIfNoMatchFound">False by default, allows the method to throw if no match under <see cref="maxStandardDeviations"/>
        /// is found.</param>
        public static SelectionWithMeanStandardDeviation<T?> SelectPreferenceWithMaxDeviationThreshold<T>(this IEnumerable<T> preferences,
            Func<T, double> valueSelector, double maxStandardDeviations = 1.5, bool throwIfNoMatchFound = false)
        {
            var preferenceList = preferences.ToList();
            var (mean, standardDeviation) = preferenceList.Select(valueSelector).MeanStandardDeviation();
            if(double.IsNaN(mean) || double.IsNaN(standardDeviation)) 
                return new SelectionWithMeanStandardDeviation<T?>(default, mean, standardDeviation);

            var minimumDeviation = double.MaxValue;
            var leastDeviated = default(T?);
            foreach (var preference in preferenceList)
            {
                var deviation = Math.Abs(valueSelector(preference) - mean);
                if (deviation < maxStandardDeviations * standardDeviation)
                    return new SelectionWithMeanStandardDeviation<T?>(preference, mean, standardDeviation);

                if (deviation > minimumDeviation) continue;
                minimumDeviation = deviation;
                leastDeviated = preference;
            }

            if(throwIfNoMatchFound) throw new InvalidDataException($"Failed to find a valid value from list within {maxStandardDeviations} " +
                $"standard deviations of the mean, with mean {mean} and standardDeviation {standardDeviation}");

            return new SelectionWithMeanStandardDeviation<T?>(leastDeviated, mean, standardDeviation);
        }

        public static string ToCsvDistinctList<T>(this IEnumerable<T> items, bool spacing = false)
        {
            var separator = "," + (spacing ? " " : string.Empty);
            return string.Join(separator, items.Select(i => i!.ToString()!.ToLowerInvariant().Trim(' ')).Distinct());
        }
    }
}