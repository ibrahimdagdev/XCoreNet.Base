using Microsoft.AspNetCore.Http;

namespace XCoreNet.Base.Utilities
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            var forwardedFor = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor;

            return context?.Connection.RemoteIpAddress?.ToString();
        }
    }
}
