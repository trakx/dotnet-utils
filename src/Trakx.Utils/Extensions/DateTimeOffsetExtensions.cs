using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Trakx.Utils.DateTimeHelpers;

namespace Trakx.Utils.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToIso8601(this DateTimeOffset offset)
        {
            return offset.DateTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset Round(this DateTimeOffset offset, TimeSpan timeSpan)
        {
            var offsetUtcDateTime = offset.UtcDateTime;
            var roundDateTime = new DateTime((offsetUtcDateTime.Ticks + timeSpan.Ticks / 2 - 1) / timeSpan.Ticks * timeSpan.Ticks, offsetUtcDateTime.Kind);
            return new DateTimeOffset(roundDateTime);
        }

        public static DateTimeOffset Round(this DateTimeOffset offset, Period period) =>
            Round(offset, period.ToTimeSpan());


        public static List<DateTimeOffset> GetDatesUntil(this DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (startDate > endDate) return new List<DateTimeOffset>();
            var count = 1 + endDate.Date.Subtract(startDate.Date).Days;
            var dates = Enumerable.Range(0, count)
                .Select(offset => new DateTimeOffset(startDate.Date.AddDays(offset), TimeSpan.Zero))
                .ToList();
            return dates;
        }
    }
}
