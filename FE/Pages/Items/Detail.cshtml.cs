using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BussinessObjects.Models;
using System.Text.Json;

namespace FE.Pages.Items
{
    public class DetailModel : PageModel
    {
        private readonly ILogger<DetailModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailModel(ILogger<DetailModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Item? Item { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"api/item/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Item not found.";
                    return NotFound();
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the response which has { message: "...", data: {...} } structure
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    Item = JsonSerializer.Deserialize<Item>(dataProperty.GetRawText(), jsonOptions);
                }

                if (Item == null)
                {
                    ErrorMessage = "Item not found.";
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item details");
                ErrorMessage = "An error occurred while loading item details.";
                return NotFound();
            }
        }
    }
}
