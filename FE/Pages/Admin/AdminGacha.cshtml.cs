using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
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

        public string ErrorMessage { get; set; } = string.Empty;
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

        public async Task<IActionResult> OnPostCreateAsync(string name, string description, string type, string rarity, IFormFile imageFile, string statsLinesStr)
        {
            var client = _httpClientFactory.CreateClient("Api");
            using var formContent = new MultipartFormDataContent();

            formContent.Add(new StringContent(name ?? ""), "Name");
            formContent.Add(new StringContent(description ?? ""), "Description");
            formContent.Add(new StringContent(type ?? ""), "Type");
            formContent.Add(new StringContent(rarity ?? ""), "Rarity");

            if (!string.IsNullOrWhiteSpace(statsLinesStr))
            {
                var statsList = statsLinesStr.Split(',').Select(s => s.Trim()).ToList();
                foreach (var stat in statsList)
                {
                    formContent.Add(new StringContent(stat), "StatsLines");
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                formContent.Add(streamContent, "ImageFile", imageFile.FileName);
            }

            var response = await client.PostAsync("api/admin/gacha/items", formContent);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(Guid id, string name, string description, string type, string rarity, string imagePath, IFormFile imageFile, string statsLinesStr)
        {
            var client = _httpClientFactory.CreateClient("Api");
            using var formContent = new MultipartFormDataContent();

            formContent.Add(new StringContent(name ?? ""), "Name");
            formContent.Add(new StringContent(description ?? ""), "Description");
            formContent.Add(new StringContent(type ?? ""), "Type");
            formContent.Add(new StringContent(rarity ?? ""), "Rarity");
            formContent.Add(new StringContent(imagePath ?? ""), "ImagePath");

            if (!string.IsNullOrWhiteSpace(statsLinesStr))
            {
                var statsList = statsLinesStr.Split(',').Select(s => s.Trim()).ToList();
                foreach (var stat in statsList)
                {
                    formContent.Add(new StringContent(stat), "StatsLines");
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                formContent.Add(streamContent, "ImageFile", imageFile.FileName);
            }

            var response = await client.PutAsync($"api/admin/gacha/items/{id}", formContent);

            return RedirectToPage();
        }
    }

    public class GachaItemDto
    {
        public Guid Id { get; set; }
        // Đã gán string.Empty để fix cảnh báo CS8618
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public List<string> StatsLines { get; set; } = new List<string>();
    }

}