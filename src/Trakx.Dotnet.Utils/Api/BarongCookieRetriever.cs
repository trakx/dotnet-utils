using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Trakx.Dotnet.Utils.Api
{
    public class BarongCookieRetriever : ICookieRetriever
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BarongCookieRetriever> _logger;

        public BarongCookieRetriever(IHttpContextAccessor httpContextAccessor, ILogger<BarongCookieRetriever> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetCookie()
        {
            if (_httpContextAccessor.HttpContext != null 
                && _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("_barong_session", out var cookie))
            {
                _logger.LogDebug("_barong_session Cookie found.");
                return cookie!;
            }
            _logger.LogDebug("_barong_session Cookie not found.");
            return "";
        }

        public string GetUserAgent()
        {
            if (_httpContextAccessor.HttpContext != null 
                && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                _logger.LogDebug("User-Agent {0} header found.", userAgent);
                return userAgent;
            }
            _logger.LogDebug("User-Agent header not found.");
            return "";
        }
    }
}
