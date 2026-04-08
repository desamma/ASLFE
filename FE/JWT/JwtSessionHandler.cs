using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ASLFE.JWT
{
    public class JwtSessionHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtSessionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
            var isAuthEndpoint = requestPath.Contains("/api/auth/register", StringComparison.OrdinalIgnoreCase) ||
                                requestPath.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase);

            if (isAuthEndpoint)
                return await base.SendAsync(request, cancellationToken);

            var httpCtx = _httpContextAccessor.HttpContext;
            var token = httpCtx?.Session.GetString("AccessToken");

            if (!string.IsNullOrWhiteSpace(token) && !isAuthEndpoint)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            // Check if we got a 401 Unauthorized response (token expired)
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && httpCtx != null)
            {
                // Clear the expired token from session
                httpCtx.Session.Remove("AccessToken");
                httpCtx.Session.Clear();

                // Sign out the user to clear authentication cookies
                await httpCtx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return response;
        }
    }
}
