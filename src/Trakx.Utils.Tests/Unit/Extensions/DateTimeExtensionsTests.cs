using System;
using System.Linq;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void GetLondonClosingTimeForDay_should_handle_daylight_savings()
        {
            var daylightSavingStart = new DateTime(2020, 03, 29, 0, 0, 0, DateTimeKind.Utc);
            var startTime = daylightSavingStart.AddDays(-2);
            var endTime = daylightSavingStart.AddDays(2);
            var distanceInHours = (int)(endTime - startTime).TotalHours + 1;
            var oneHour = TimeSpan.FromHours(1);

            var realHours = Enumerable.Range(0, distanceInHours)
                .Select(i => startTime.Add(oneHour.Multiply(i))).ToList();

            var effectiveHours =
                realHours.Select(h => new {DateTime = h, Closing = h.GetLondonClosingTimeForDay()})
                    .ToList();

            effectiveHours.Count(k => k.Closing.TimeOfDay.Equals(TimeSpan.FromHours(17))).Should().Be(24 * 2 + 1);
            effectiveHours.Count(k => k.Closing.TimeOfDay.Equals(TimeSpan.FromHours(18))).Should().Be(24 * 2);
        }
    }
}
