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
            var roundDateTime =
                new DateTime((offsetUtcDateTime.Ticks + timeSpan.Ticks / 2 - 1) / timeSpan.Ticks * timeSpan.Ticks,
                    offsetUtcDateTime.Kind);
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

        /// <summary>
        /// Get the Date part of the Date time, represented in UTC.
        /// </summary>
        /// <param name="dateTime">DateTime for which we need the date part.</param>
        /// <returns>The UTC date as of <see cref="dateTime"/></returns>
        public static DateTimeOffset UtcDate(this DateTimeOffset dateTime) =>
            new(dateTime.UtcDateTime.Date, TimeSpan.Zero);
    }
}
