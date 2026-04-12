using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using BussinessObjects.Models;

namespace FE.Pages.News
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
    }
}
