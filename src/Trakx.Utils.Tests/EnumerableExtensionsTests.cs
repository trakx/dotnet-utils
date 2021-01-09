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
        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_value_if_not_too_deviant()
        {
            var numbers = new double[] {1.0, 1.3, 1.2, 0.9, 0.8, 0.8, 1.2};
            var (mean, standardDeviation) = numbers.MeanStandardDeviation();
            
            Math.Abs(numbers[0] - mean).Should().BeLessOrEqualTo(standardDeviation);

            var selection = numbers.SelectPreferenceWithMaxDeviationThreshold(x => x, 1);
            
            selection.Should().Be(numbers[0]);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_choose_first_values_if_too_deviant()
        {
            var numbers = new double[] { 1.3, 1.2, 1.0, 0.9, 0.8, 0.8, 1.2 };
            var (mean, standardDeviation) = numbers.MeanStandardDeviation();
            var maxStandardDeviation = 0.5;

            Math.Abs(numbers[0] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);
            Math.Abs(numbers[1] - mean).Should().BeGreaterOrEqualTo(standardDeviation * maxStandardDeviation);

            var selection = numbers.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation);

            selection.Should().Be(numbers[2]);
        }

        [Fact]
        public void SelectPreferenceWithMaxDeviationThreshold_should_throw_when_no_value_matches()
        {
            var numbers = new double[] { 1.3, 1.2, 1.0, 0.9, 0.8, 0.8, 1.2 };
            var (mean, standardDeviation) = numbers.MeanStandardDeviation();
            var maxStandardDeviation = 0.01;

            numbers.Select(x => Math.Abs(x - mean)).All(d => d > standardDeviation * maxStandardDeviation)
                .Should().BeTrue();
            
            var selectionAction = new Action(() => numbers.SelectPreferenceWithMaxDeviationThreshold(x => x, maxStandardDeviation));

            selectionAction.Should().Throw<InvalidDataException>();
        }
    }
}
