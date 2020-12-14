using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Trakx.Dotnet.Utils.Extensions
{
    public static class StringExtensions
    {
        public static Regex EthereumAddressRegex = new Regex(@"^(?<Prefix>0x)(?<Address>[A-F,a-f,0-9]{40})$");

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
            return BitConverter.ToString(array).Replace("-", "").ToLower();
        }

        public static List<string> SplitCsvToLowerCaseDistinctList(this string csvString)
        {
            if (string.IsNullOrWhiteSpace(csvString)) return new List<string>();
            var values = csvString
                .Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.ToLowerInvariant());
            return values.Distinct().ToList();
        }
    }
}
