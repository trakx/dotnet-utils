using System;
using System.Globalization;

namespace Trakx.Dotnet.Utils.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToIso8601(this DateTimeOffset offset)
        {
            return offset.DateTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
