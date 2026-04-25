using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {

        }

        public string ResolveImageUrl(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return string.Empty;

            if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absoluteUri))
                return absoluteUri.ToString();

            var bucket = _configuration["Firebase:StorageBucket"];
            if (string.IsNullOrWhiteSpace(bucket))
                return imagePath.StartsWith('/') ? imagePath : $"/{imagePath}";

            var normalizedPath = imagePath.Replace('\\', '/').TrimStart('/');

            return $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString(normalizedPath)}?alt=media";
        }
    }
}
