using Microsoft.AspNetCore.Mvc.RazorPages;
using BussinessObjects.Models;
using System.Text.Json;

namespace FE.Pages.Items
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public List<Item> Items { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public List<string> AvailableTypes { get; set; } = new();
        public List<string> AvailableRarities { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/item");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Failed to load items.";
                    return;
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                List<Item>? items = null;

                // Deserialize the response which has { message: "...", data: [...] } structure
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    items = JsonSerializer.Deserialize<List<Item>>(dataProperty.GetRawText(), jsonOptions);
                }

                if (items != null && items.Count > 0)
                {
                    Items = items;
                    
                    // Extract unique types and rarities
                    AvailableTypes = Items
                        .Select(i => i.Type)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList();

                    AvailableRarities = Items
                        .Select(i => i.Rarity)
                        .Distinct()
                        .OrderBy(r => r)
                        .ToList();
                }
                else
                {
                    ErrorMessage = "No items available.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading items");
                ErrorMessage = "An error occurred while loading items.";
            }
        }
    }
}
