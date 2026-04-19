using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace FE.Pages.Inventory;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<UserItemDetail> UserItems { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.GetAsync($"api/useritem/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to load inventory.";
                return Page();
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var content = await response.Content.ReadAsStringAsync();

            // Deserialize the response which has { message: "...", data: [...], success: true } structure
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
            if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
            {
                if (dataProperty.ValueKind == JsonValueKind.Array)
                {
                    var userItemDtos = JsonSerializer.Deserialize<List<UserItemDto>>(dataProperty.GetRawText(), jsonOptions) ?? new();
                    
                    // Transform DTOs to UI models
                    UserItems = userItemDtos.Select(ui => new UserItemDetail
                    {
                        UserId = ui.UserId,
                        ItemId = ui.ItemId,
                        Quantity = ui.Quantity,
                        QuantityDelivered = ui.QuantityDelivered,
                        IsDeliveredToGame = ui.IsDeliveredToGame,
                        DeliveredToGameAt = ui.DeliveredToGameAt,
                        CreatedAt = ui.CreatedAt,
                        Name = ui.Item?.Name ?? "Unknown",
                        Description = ui.Item?.Description ?? string.Empty,
                        Type = ui.Item?.Type ?? "Unknown",
                        Rarity = ui.Item?.Rarity ?? "Common",
                        ImagePath = ui.Item?.ImagePath ?? string.Empty,
                        StatsLines = new List<string>() // Stats are not included in the API response
                    }).ToList();
                }
            }

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while loading your inventory: {ex.Message}";
            return Page();
        }
    }

    public class UserItemDetail
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public List<string> StatsLines { get; set; } = new();
        public int Quantity { get; set; }
        public int QuantityDelivered { get; set; }
        public int QuantityPending => Quantity - QuantityDelivered;
        public bool IsDeliveredToGame { get; set; }
        public DateTime? DeliveredToGameAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserItemDto
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public int QuantityDelivered { get; set; }
        public int QuantityPending { get; set; }
        public bool IsDeliveredToGame { get; set; }
        public DateTime? DeliveredToGameAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public ItemDto? Item { get; set; }
    }

    public class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
