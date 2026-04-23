using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FE.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class AdminUsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminUsersModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchName { get; set; }

        public string ErrorMessage { get; set; }
        public List<AdminUserDto> Users { get; set; } = new List<AdminUserDto>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            var url = "api/admin/users";

            if (!string.IsNullOrEmpty(SearchName))
            {
                url += $"?searchName={Uri.EscapeDataString(SearchName)}";
            }

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<AdminUserDto>>>();
                    if (result != null && result.Success)
                    {
                        Users = result.Data ?? new List<AdminUserDto>();
                    }
                }
                else
                {
                    ErrorMessage = "Lỗi khi tải danh sách người dùng từ Server.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể kết nối đến máy chủ: " + ex.Message;
            }
        }
        public async Task<IActionResult> OnGetDetailAsync(Guid userId)
        {
            var client = _httpClientFactory.CreateClient("Api");
            try
            {
                var response = await client.GetAsync($"api/admin/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminUserDto>>();
                    if (result != null && result.Success && result.Data != null)
                    {
                        return new JsonResult(result.Data);
                    }
                }
                return new JsonResult(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostToggleBanAsync(Guid userId)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PutAsync($"api/admin/users/{userId}/toggle-ban", null);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAdjustCurrencyAsync(Guid userId, int amountChange)
        {
            var client = _httpClientFactory.CreateClient("Api");

            var requestBody = new { AmountChange = amountChange };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/admin/users/{userId}/adjust-currency", content);
            return RedirectToPage();
        }
    }

    // --- Các class phụ trợ mapping dữ liệu Json ---
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int CurrencyAmount { get; set; }
        public int PityCounter { get; set; }
        public bool IsBanned { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}