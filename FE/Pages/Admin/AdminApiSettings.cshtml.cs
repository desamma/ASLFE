using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class AdminApiSettingsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminApiSettingsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ApiSettingDto> SettingsList { get; set; } = new List<ApiSettingDto>();

        [TempData]
        public string Message { get; set; }
        [TempData]
        public bool IsSuccess { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.GetAsync("api/admin/settings/api-keys");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ApiSettingDto>>>();
                if (result != null && result.Success && result.Data != null)
                {
                    SettingsList = result.Data;
                }
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(string geminiKey, string colabUrl)
        {
            var newSetting = new ApiSettingDto { GeminiApiKey = geminiKey, ColabApiUrl = colabUrl };
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("api/admin/settings/api-keys", newSetting);

            if (response.IsSuccessStatusCode)
            {
                IsSuccess = true;
                Message = "Thêm API mới thành công!";
            }
            else
            {
                IsSuccess = false;
                Message = "Lỗi khi thêm API!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.DeleteAsync($"api/admin/settings/api-keys/{id}");

            if (response.IsSuccessStatusCode)
            {
                IsSuccess = true;
                Message = "Xóa API thành công!";
            }
            else
            {
                IsSuccess = false;
                Message = "Lỗi khi xóa API!";
            }
            return RedirectToPage();
        }
        public class ApiSettingDto
        {
            public Guid Id { get; set; }
            public string? GeminiApiKey { get; set; }
            public string? ColabApiUrl { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
