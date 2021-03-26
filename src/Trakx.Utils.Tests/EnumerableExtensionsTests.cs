using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using MathNet.Numerics.Statistics;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests
{
    public class EnumerableExtensionsTests
    {
        private readonly double[] _distribution;

        public EnumerableExtensionsTests()
        {
            _distribution = new [] {1.3, 1.2, 1.0, 0.9, 0.8, 0.8, 1.2};
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_value_if_not_too_deviant()
        {
            var (mean, standardDeviation) = _distribution.MeanStandardDeviation();
            
            Math.Abs(_distribution[0] - mean).Should().BeLessOrEqualTo(3 * standardDeviation);

            var selection = _distribution.SelectPreferenceWithMaxDeviationThreshold(x => x, 3);
            
            selection.Selection.Should().Be(_distribution[0]);
            selection.Mean.Should().BeApproximately(1.0285714285714285, double.Epsilon);
            selection.StandardDeviation.Should().BeApproximately(0.20586634591635514, double.Epsilon);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_values_if_too_deviant()
        {
            var (mean, standardDeviation) = _distribution.MeanStandardDeviation();
            var maxStandardDeviation = 0.5;

            Math.Abs(_distribution[0] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);
            Math.Abs(_distribution[1] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);

            var selection = _distribution.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation);

            selection.Selection.Should().Be(_distribution[2]);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_throw_when_asked_and_no_value_matches()
        {
            var maxStandardDeviation = GetTooStrictMaxStandardDeviation();

            var selectionAction = new Action(() => _distribution.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation, true));

            selectionAction.Should().Throw<InvalidDataException>();
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_when_not_asked_and_no_value_matches()
        {
            var maxStandardDeviation = GetTooStrictMaxStandardDeviation();

            var selectionAction = new Func<EnumerableExtensions.SelectionWithMeanStandardDeviation<double?>>(
                () => _distribution.Select(x => new double?(x))
                    .SelectPreferenceWithMaxDeviationThreshold(x => x!.Value, maxStandardDeviation));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(1);
        }

        private double GetTooStrictMaxStandardDeviation()
        {
            var (mean, standardDeviation) = _distribution.MeanStandardDeviation();
            var maxStandardDeviation = 0.01;

            _distribution.Select(x => Math.Abs(x - mean)).All(d => d > standardDeviation * maxStandardDeviation)
                .Should().BeTrue();
            return maxStandardDeviation;
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_on_empty_sets()
        {
            var selectionAction = new Func<EnumerableExtensions.SelectionWithMeanStandardDeviation<double?>>(
                () =>  Array.Empty<double?>().SelectPreferenceWithMaxDeviationThreshold(x => x ?? 0, 10));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(null);
        }


        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_on_sets_with_one_value()
        {
            var selectionAction = new Func<EnumerableExtensions.SelectionWithMeanStandardDeviation<double?>>(
                () => new double?[]{ 0.245, double.NaN, null}.SelectPreferenceWithMaxDeviationThreshold(
                    x => x ?? 0, 10));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(0.245);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_not_throw_on_sets_with_two_values()
        {
            var selectionAction = new Func<EnumerableExtensions.SelectionWithMeanStandardDeviation<double?>>(
                () => new double?[] { 0.245, 0.256, null }.SelectPreferenceWithMaxDeviationThreshold(
                    x => x ?? 0, 10));

            selectionAction.Should().NotThrow<Exception>();
            selectionAction.Invoke().Selection.Should().Be(0.245);
        }



        [Fact]
        public void ToCsvDistinctList_should_join_trimmed_lower_cased_ToString_results_with_spacing()
        {
            var strings = new[] {"ab ", "def", " klm", "KlM"};
            strings.ToCsvDistinctList(true).Should().Be("ab, def, klm");
        }
    }
}
