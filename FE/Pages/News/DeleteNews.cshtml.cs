using BussinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FE.Pages.News
{
    public class DeleteNewsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteNewsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public GameNews? News { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync($"api/gamenews/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "News not found.";
                    return NotFound();
                }

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    News = JsonSerializer.Deserialize<GameNews>(dataProperty.GetRawText(), jsonOptions);
                }

                if (News == null)
                {
                    ErrorMessage = "News not found.";
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.DeleteAsync($"api/gamenews/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/News/Index");
                }

                ErrorMessage = "Failed to delete news. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting: {ex.Message}";
                return Page();
            }
        }
    }
}
