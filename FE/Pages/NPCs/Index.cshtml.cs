using Microsoft.AspNetCore.Mvc.RazorPages;
using BussinessObjects.Models;
using System.Text.Json;

namespace FE.Pages.NPCs
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

        public List<NPC> NPCs { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public List<string> AvailableTypes { get; set; } = new();
        public List<string> AvailableLocations { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/npc");
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, jsonOptions);
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Failed to load NPCs.";
                    return;
                }

                List<NPC>? npcs = null;

                // Deserialize the response which has { message: "...", data: [...] } structure
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("data", out var dataProperty))
                {
                    npcs = JsonSerializer.Deserialize<List<NPC>>(dataProperty.GetRawText(), jsonOptions);
                }

                if (npcs != null && npcs.Count > 0)
                {
                    NPCs = npcs;
                    
                    // Extract unique types and locations
                    AvailableTypes = NPCs
                        .Select(n => n.NPCType)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList();

                    AvailableLocations = NPCs
                        .Where(n => !string.IsNullOrEmpty(n.Location))
                        .Select(n => n.Location!)
                        .Distinct()
                        .OrderBy(l => l)
                        .ToList();
                }
                else
                {
                    ErrorMessage = "No NPCs available.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading NPCs");
                ErrorMessage = "An error occurred while loading NPCs.";
            }
        }
    }
}
