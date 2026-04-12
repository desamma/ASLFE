using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Payment
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public string UserName { get; set; } = string.Empty;
        public long CurrentVp { get; set; }
        public string ApiBaseUrl { get; set; } = string.Empty;

        public void OnGet()
        {
            UserName = User.Identity?.Name ?? "Player";
            var vpClaim = User.FindFirst("CurrencyAmount")?.Value;
            CurrentVp = long.TryParse(vpClaim, out var vp) ? vp : 0;
            ApiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7206";
        }
    }
}