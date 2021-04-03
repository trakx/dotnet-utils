using System;
using System.Linq;
using Xunit.Abstractions;

namespace Trakx.Utils.Testing
{
    public class MockCreator
    {
        protected readonly Random Random;
        protected static readonly string AddressChars = "abcdef01234566789";
        protected static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz";
        protected readonly string Name;

        public MockCreator(ITestOutputHelper output)
        {
            Name = output.GetCurrentTestName();
            Random = new Random(Name.GetHashCode());
        }

        public string GetRandomAddressEthereum() => "0x" + new string(Enumerable.Range(0, 40)
            .Select(_ => AddressChars[Random.Next(0, AddressChars.Length)]).ToArray());
        public string GetRandomEthereumTransactionHash() => "0x" + new string(Enumerable.Range(0, 64)
                                                        .Select(_ => AddressChars[Random.Next(0, AddressChars.Length)]).ToArray());

        public string GetRandomString(int size) => new string(Enumerable.Range(0, size)
            .Select(_ => Alphabet[Random.Next(0, Alphabet.Length)]).ToArray());

        public string GetRandomYearMonthSuffix() => $"{Random.Next(20, 36):00}{Random.Next(1, 13):00}";

        public DateTime GetRandomUtcDateTime()
        {
            var dateTime = GetRandomUtcDateTimeOffset();
            return dateTime.UtcDateTime;
        }

        public ulong GetRandomUnscaledAmount() => (ulong)Random.Next(1, int.MaxValue);
        public ushort GetRandomDecimals() => (ushort)Random.Next(0, 19);

        public DateTimeOffset GetRandomUtcDateTimeOffset()
        {
            var firstJan2020 = new DateTimeOffset(2020, 01, 01, 0, 0, 0, TimeSpan.Zero);
            var firstJan2050 = new DateTimeOffset(2050, 01, 01, 0, 0, 0, 0, TimeSpan.Zero);
            var timeBetween2020And2050 = firstJan2050.Subtract(firstJan2020);

            var randomDay = firstJan2020 + TimeSpan.FromDays(Random.Next(0, (int)timeBetween2020And2050.TotalDays));
            return randomDay;
        }

        public decimal GetRandomPrice() => Random.Next(1, int.MaxValue) / 1e5m;
        public decimal GetRandomValue() => Random.Next(1, int.MaxValue) / 1e2m;
        public TimeSpan GetRandomTimeSpan() => TimeSpan.FromSeconds(Random.Next(1, (int)TimeSpan.FromDays(1000).TotalSeconds));
    }
}
