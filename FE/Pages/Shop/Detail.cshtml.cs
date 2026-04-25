using BussinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FE.Pages.Shop
{
    public class DetailModel : PageModel
    {
        private readonly ILogger<DetailModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string FirebasePrefix = "https://firebasestorage.googleapis.com/";
        private const string ItemPathSegment = "images%2Fitems%2F";
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

                ShopItem.ImagePath = Normalize(ShopItem.ImagePath);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shop item details");
                ErrorMessage = "An error occurred while loading shop item details.";
                return NotFound();
            }
        }

       
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostBuyAsync([FromBody] BuyShopItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request", errors = ModelState });
            }

            // FIX 3: Kiểm tra request null (có thể xảy ra nếu Content-Type sai)
            if (request == null)
            {
                return BadRequest(new { message = "Request body is missing or malformed." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Api");

                var response = await client.PostAsJsonAsync("api/shoppurchase/buy", request);
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
        private static string Normalize(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) ||
                !imagePath.StartsWith(FirebasePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return imagePath ?? string.Empty;
            }

            var queryIndex = imagePath.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex < 0)
            {
                return imagePath;
            }

            var basePath = imagePath[..queryIndex];
            if (!basePath.Contains(ItemPathSegment, StringComparison.OrdinalIgnoreCase) ||
                basePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                basePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                basePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                basePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            return $"{basePath}.jpg{imagePath[queryIndex..]}";
        }
    }
}
