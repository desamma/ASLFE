using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BussinessObjects.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace FE.Pages.Shop
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

        public ShopItem? ShopItem { get; set; }
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
                var response = await client.GetAsync($"api/shopitem/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Shop item not found.";
                    return NotFound();
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    ShopItem = JsonSerializer.Deserialize<ShopItem>(dataProperty.GetRawText(), jsonOptions);
                }

                if (ShopItem == null || !ShopItem.IsActive)
                {
                    ErrorMessage = "Shop item not found or is no longer available.";
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shop item details");
                ErrorMessage = "An error occurred while loading shop item details.";
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostBuyAsync([FromBody] BuyShopItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request", errors = ModelState });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                // Gọi thẳng backend API, không qua proxy
                var response = await client.PostAsJsonAsync("api/shop/buy", request);
                var content = await response.Content.ReadAsStringAsync();

                return new ContentResult
                {
                    Content = content,
                    ContentType = "application/json",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while buying shop item");
                return StatusCode(500, new { message = "Internal server error while processing purchase." });
            }
        }

        public class BuyShopItemRequest
        {
            public Guid ShopItemId { get; set; }
            public int Quantity { get; set; } = 1;
        }
    }
}