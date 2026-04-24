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

        [BindProperty]
        public ApiSettingDto Settings { get; set; } = new ApiSettingDto();

        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.GetAsync("api/admin/settings/api-keys");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApiSettingDto>>();
                if (result != null && result.Success && result.Data != null)
                {
                    Settings = result.Data;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PutAsJsonAsync("api/admin/settings/api-keys", Settings);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApiSettingDto>>();
                IsSuccess = true;
                Message = "Cập nhật cấu hình API thành công!";
                if (result?.Data != null) Settings = result.Data;
            }
            else
            {
                IsSuccess = false;
                Message = "Có lỗi xảy ra khi lưu API Key.";
            }
            return Page();
        }
    }

    // Class DTO đặt luôn ở đây cho gọn, giống các trang admin khác bạn đang làm
    public class ApiSettingDto
    {
        public string? GeminiApiKey { get; set; }
        public string? ColabApiUrl { get; set; }
    }
}
