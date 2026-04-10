using Microsoft.AspNetCore.Mvc.RazorPages;
using BussinessObjects.Models;
using System.Text.Json;

namespace FE.Pages.Shop
{
    public class ShopIndexModel : PageModel
    {
        private readonly ILogger<ShopIndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopIndexModel(ILogger<ShopIndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Dictionary<string, List<ShopItem>> ShopItemsByCategory { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/shopitem/active");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Failed to load shop items.";
                    return;
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                List<ShopItem>? items = null;

                // Deserialize the response which has { message: "...", data: [...] } structure
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    items = JsonSerializer.Deserialize<List<ShopItem>>(dataProperty.GetRawText(), jsonOptions);
                }

                if (items != null && items.Count > 0)
                {
                    // Group items by category
                    ShopItemsByCategory = items
                        .Where(item => item.IsActive)
                        .GroupBy(item => item.Category)
                        .ToDictionary(g => g.Key, g => g.ToList());
                }
                else
                {
                    ErrorMessage = "No shop items available.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shop items");
                ErrorMessage = "An error occurred while loading shop items.";
            }
        }
    }
}
