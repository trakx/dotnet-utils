using System.IO;
using DotNetEnv;
using static System.Environment;

namespace Trakx.Utils.Testing
{
    public static class Secrets
    {
        static Secrets()
        {
            var srcPath = new DirectoryInfo(CurrentDirectory).Parent?.Parent?.Parent?.Parent!;
            Env.Load(Path.Combine(srcPath.FullName, ".env"));
        }

        public static string? CryptoCompareApiKey => GetEnvironmentVariable("CryptoCompareApiConfiguration__ApiKey");
        public static string? InfuraApiKey => GetEnvironmentVariable("INFURA_API_KEY");
        public static string? EthereumWalletSecret => GetEnvironmentVariable("ETHERWALLET");

        public static string? PeatioApiKey => GetEnvironmentVariable("ExchangeApiConfiguration__ApiKey");
        public static string? PeatioApiSecret => GetEnvironmentVariable("ExchangeApiConfiguration__ApiSecret");
        public static string? GithubAccessToken => GetEnvironmentVariable("GITHUB_ACCESS_TOKEN");
    }
}
