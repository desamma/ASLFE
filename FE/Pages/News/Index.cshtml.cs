using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using BussinessObjects.Models;

namespace FE.Pages.News
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<GameNews> NewsList { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/gamenews");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Failed to load news.";
                    return Page();
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                
                List<GameNews>? newsList = null;

                // Deserialize the response which has { message: "...", data: [...] } structure
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    newsList = JsonSerializer.Deserialize<List<GameNews>>(dataProperty.GetRawText(), jsonOptions);
                }

                if (newsList != null)
                {
                    // Apply search filter if provided
                    if (!string.IsNullOrWhiteSpace(SearchTerm))
                    {
                        var searchLower = SearchTerm.ToLower();
                        NewsList = newsList
                            .Where(n => n.Title.ToLower().Contains(searchLower) ||
                                       n.Description.ToLower().Contains(searchLower))
                            .OrderByDescending(n => n.Id)
                            .ToList();
                    }
                    else
                    {
                        NewsList = newsList.OrderByDescending(n => n.Id).ToList();
                    }
                }

                ViewData["Title"] = "Game News";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        public string ResolveBannerUrl(string? bannerPath)
        {
            if (string.IsNullOrWhiteSpace(bannerPath))
                return string.Empty;

            if (Uri.TryCreate(bannerPath, UriKind.Absolute, out var absoluteUri))
                return absoluteUri.ToString();

            var bucket = _configuration["Firebase:StorageBucket"];
            if (string.IsNullOrWhiteSpace(bucket))
                return $"/images/{bannerPath}";

            var normalizedPath = bannerPath.Replace('\\', '/').TrimStart('/');
            if (normalizedPath.StartsWith("News/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath[5..];
            }

            return $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString(normalizedPath)}?alt=media";
        }
    }
}
