using System;

namespace Trakx.Dotnet.Utils.Api
{
    public static class StringExtensions
    {
        public static string ToHexString(this byte[] array)
        {
            return BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
        }
    }
}
