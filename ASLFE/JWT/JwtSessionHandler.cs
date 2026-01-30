using System.Net.Http.Headers;

namespace ASLFE.JWT
{
    public class JwtSessionHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtSessionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
            var isAuthEndpoint = requestPath.Contains("/api/auth/register", StringComparison.OrdinalIgnoreCase)||
                                requestPath.Contains("/api/auth/signin", StringComparison.OrdinalIgnoreCase);

            if(isAuthEndpoint) return base.SendAsync(request, cancellationToken);

            var httpCtx = _httpContextAccessor.HttpContext;
            var token = httpCtx?.Session.GetString("AccessToken");


            if (!string.IsNullOrWhiteSpace(token) && !isAuthEndpoint)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
