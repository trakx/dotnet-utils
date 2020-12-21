using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Trakx.Utils.Api
{
    public class GoogleCredentialProvider : ICredentialsProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GoogleCredentialProvider> _logger;

        public GoogleCredentialProvider(IHttpContextAccessor httpContextAccessor, ILogger<GoogleCredentialProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <inheritdoc />
        #region Implementation of ICredentialsProvider
        public void AddCredentials(HttpRequestMessage msg)
        {
            var token = GetJsonWebToken().GetAwaiter().GetResult();
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        #endregion

        private async Task<string> GetJsonWebToken()
        {
            var auth = await (_httpContextAccessor.HttpContext?.AuthenticateAsync()
                              ?? Task.FromResult(AuthenticateResult.NoResult()));
            var token = auth.Properties?.GetTokenValue(OpenIdConnectParameterNames.IdToken);
            _logger.LogDebug(string.IsNullOrEmpty(token) ? "Google token not found." : "Google token found.");
            return token ?? "";
        }
    }
}