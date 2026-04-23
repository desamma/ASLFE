using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FE.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class AdminGachaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminGachaModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string ErrorMessage { get; set; }
        public List<GachaItemDto> Items { get; set; } = new List<GachaItemDto>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            try
            {
                var response = await client.GetAsync("api/admin/gacha/items");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<GachaItemDto>>>();
                    if (result != null && result.Success)
                    {
                        Items = result.Data ?? new List<GachaItemDto>();
                    }
                }
                else
                {
                    ErrorMessage = "Lỗi khi tải danh sách vật phẩm.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối: " + ex.Message;
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(string name, string description, string type, string rarity, string imagePath, string statsLinesStr)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var statsList = string.IsNullOrWhiteSpace(statsLinesStr)
                ? new List<string>()
                : statsLinesStr.Split(',').Select(s => s.Trim()).ToList();

            var requestBody = new
            {
                Name = name,
                Description = description,
                Type = type,
                Rarity = rarity,
                ImagePath = imagePath,
                StatsLines = statsList
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/admin/gacha/items", content);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid id, string name, string description, string type, string rarity, string imagePath, string statsLinesStr)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var statsList = string.IsNullOrWhiteSpace(statsLinesStr)
                ? new List<string>()
                : statsLinesStr.Split(',').Select(s => s.Trim()).ToList();

            var requestBody = new
            {
                Name = name,
                Description = description,
                Type = type,
                Rarity = rarity,
                ImagePath = imagePath,
                StatsLines = statsList
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/admin/gacha/items/{id}", content);

            return RedirectToPage();
        }
    }

    public class GachaItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public string ImagePath { get; set; }
        public List<string> StatsLines { get; set; } = new List<string>();
    }
}