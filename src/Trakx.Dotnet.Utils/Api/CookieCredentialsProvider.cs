using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Trakx.Dotnet.Utils.Api
{
    public class CookieCredentialsProvider : ICredentialsProvider
    {
        private const string BarongCookieKey = "_barong_session";
        private const string UserAgentKey = "User-Agent";
        private readonly ICookieRetriever _cookieRetriever;

        public CookieCredentialsProvider(ICookieRetriever cookieRetriever)
        {
            _cookieRetriever = cookieRetriever;
        }

        #region Implementation of ICredentialsProvider

        /// <inheritdoc />
        public void AddCredentials(HttpRequestMessage msg)
        {
            msg.Headers.Add(UserAgentKey, _cookieRetriever.GetUserAgent());
            msg.Headers.Add("Cookie", $"{BarongCookieKey}={_cookieRetriever.GetCookie()}");
        }

        #endregion
    }

    public class MockCookieCredentialsProvider : ICredentialsProvider
    {
        private readonly MockBarongJwtTokens _mockBarongJwt;

        public MockCookieCredentialsProvider(ICookieRetriever cookieRetriever)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            {
                throw new InvalidOperationException($"{nameof(MockCookieCredentialsProvider)} should not be used in non development environment.");
            }
            _mockBarongJwt = new MockBarongJwtTokens();
        }

        #region Implementation of ICredentialsProvider

        /// <inheritdoc />
        public void AddCredentials(HttpRequestMessage msg)
        {
            var token = _mockBarongJwt.GenerateJwtToken();
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        #endregion
    }
}