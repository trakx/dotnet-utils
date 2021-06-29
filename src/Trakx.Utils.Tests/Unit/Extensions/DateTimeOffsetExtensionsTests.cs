using System;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class DateTimeOffsetExtensionsTests
    {
        [Theory]
        [InlineData(true, "2021-06-10T22:30:00.00+02:00", "2021-06-10T20:30:00.0000000+00:00")]
        [InlineData(false, "2021-06-10T22:30:00.00+02:00", "2021-06-10T22:30:00.0000000+02:00")]
        [InlineData(true, "2021-06-10z", "2021-06-10T00:00:00.0000000+00:00")]
        public void ToIso8601_should_convert_to_utc_when_asked(bool convertToUtc, string fromDate, string toDate)
        {
            DateTimeOffset.Parse(fromDate).ToIso8601(convertToUtc).Should().Be(toDate);
        }
    }
}