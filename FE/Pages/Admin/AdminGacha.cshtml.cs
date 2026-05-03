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

        public List<GachaBannerDto> BannerList { get; set; } = new();

        [TempData] public string? Message { get; set; }
        [TempData] public bool IsSuccess { get; set; }

        // ── GET: Load all banners ─────────────────────────────────────
        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                var response = await client.GetAsync("api/admin/gacha/banners");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<GachaBannerDto>>>();
                    if (result?.Data != null)
                    {
                        BannerList = result.Data;
                        Console.WriteLine($"[AdminGacha] Loaded {BannerList.Count} banners");
                    }
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AdminGacha] GET banners failed: {response.StatusCode} - {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminGacha] Exception in OnGetAsync: {ex.Message}");
            }
        }

        // ── POST: Create Banner ───────────────────────────────────────
        public async Task<IActionResult> OnPostCreateBannerAsync(
            string name, string? description,
            IFormFile? bannerImage, string? bannerImagePath,
            int costPerSinglePull, int costPerMultiPull,
            int pityThreshold, int hardPityThreshold,
            DateTime startDate, DateTime endDate)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(name), "Name");
            content.Add(new StringContent(description ?? ""), "Description");
            content.Add(new StringContent(costPerSinglePull.ToString()), "CostPerSinglePull");
            content.Add(new StringContent(costPerMultiPull.ToString()), "CostPerMultiPull");
            content.Add(new StringContent(pityThreshold.ToString()), "PityThreshold");
            content.Add(new StringContent(hardPityThreshold.ToString()), "HardPityThreshold");
            content.Add(new StringContent(startDate.ToString("O")), "StartDate");
            content.Add(new StringContent(endDate.ToString("O")), "EndDate");

            // Ưu tiên: file upload > URL/path
            if (bannerImage != null && bannerImage.Length > 0)
            {
                var fileContent = new StreamContent(bannerImage.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(bannerImage.ContentType)
                        ? "image/jpeg"
                        : bannerImage.ContentType);
                // Key phải khớp với [FromForm] ImageFile trong BE DTO
                content.Add(fileContent, "ImageFile", bannerImage.FileName);
            }
            else if (!string.IsNullOrWhiteSpace(bannerImagePath))
            {
                content.Add(new StringContent(bannerImagePath), "BannerImagePath");
            }

            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsync("api/admin/gacha/banners", content);

            IsSuccess = response.IsSuccessStatusCode;
            if (!IsSuccess)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Message = TryExtractMessage(errorBody, "Lỗi khi tạo banner!");
                Console.WriteLine($"[AdminGacha] Create banner failed: {response.StatusCode} - {errorBody}");
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AdminGacha] Banner created: {body}");
                Message = "Tạo banner thành công!";
            }

            return RedirectToPage();
        }

        // ── POST: Update Banner ───────────────────────────────────────
        // FIX 1: Thêm pityThreshold & hardPityThreshold vào signature
        public async Task<IActionResult> OnPostUpdateBannerAsync(
            Guid bannerId,
            string name, string? description,
            IFormFile? bannerImageFile, string? bannerImagePath,
            int costPerSinglePull, int costPerMultiPull,
            int pityThreshold,      // ← FIX 1: thêm
            int hardPityThreshold,  // ← FIX 1: thêm
            DateTime startDate, DateTime endDate)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(name), "Name");
            content.Add(new StringContent(description ?? ""), "Description");
            content.Add(new StringContent(costPerSinglePull.ToString()), "CostPerSinglePull");
            content.Add(new StringContent(costPerMultiPull.ToString()), "CostPerMultiPull");
            content.Add(new StringContent(pityThreshold.ToString()), "PityThreshold");     // ← FIX 1
            content.Add(new StringContent(hardPityThreshold.ToString()), "HardPityThreshold"); // ← FIX 1
            content.Add(new StringContent(startDate.ToString("O")), "StartDate");
            content.Add(new StringContent(endDate.ToString("O")), "EndDate");

            // FIX 2: Key gửi lên phải là "ImageFile" để khớp [FromForm] trong UpdateGachaBannerRequest
            if (bannerImageFile != null && bannerImageFile.Length > 0)
            {
                var fileContent = new StreamContent(bannerImageFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(bannerImageFile.ContentType)
                        ? "image/jpeg"
                        : bannerImageFile.ContentType);
                content.Add(fileContent, "ImageFile", bannerImageFile.FileName); // ← FIX 2: key = "ImageFile"
            }
            else if (!string.IsNullOrWhiteSpace(bannerImagePath))
            {
                content.Add(new StringContent(bannerImagePath), "BannerImagePath");
            }
            // Nếu cả 2 đều null → giữ ảnh cũ (backend xử lý allowEmpty: true)

            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PutAsync($"api/admin/gacha/banners/{bannerId}", content);

            IsSuccess = response.IsSuccessStatusCode;
            if (!IsSuccess)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Message = TryExtractMessage(errorBody, "Lỗi khi cập nhật banner!");
                Console.WriteLine($"[AdminGacha] Update banner failed: {response.StatusCode} - {errorBody}");
            }
            else
            {
                Message = "Cập nhật banner thành công!";
            }

            return RedirectToPage();
        }

        // ── POST: Toggle Banner Active ────────────────────────────────
        public async Task<IActionResult> OnPostToggleAsync(Guid bannerId)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PatchAsync(
                $"api/admin/gacha/banners/{bannerId}/toggle", null);

            IsSuccess = response.IsSuccessStatusCode;
            Message = IsSuccess ? "Đã thay đổi trạng thái banner!" : "Lỗi khi toggle banner!";
            return RedirectToPage();
        }

        // ── POST: Add Item to Banner ──────────────────────────────────
        public async Task<IActionResult> OnPostAddItemAsync(
            Guid bannerId, Guid itemId, int starRating,
            double dropRate, string? itemCategory, bool isFeatured)
        {
            if (itemId == Guid.Empty)
            {
                IsSuccess = false;
                Message = "Vui lòng chọn item trước khi thêm!";
                return RedirectToPage();
            }

            var payload = new AddGachaItemRequest
            {
                ItemId = itemId,
                StarRating = starRating,
                DropRate = dropRate,
                ItemCategory = itemCategory ?? "",
                IsFeatured = isFeatured
            };

            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync(
                $"api/admin/gacha/banners/{bannerId}/items", payload);

            IsSuccess = response.IsSuccessStatusCode;
            if (!IsSuccess)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Message = TryExtractMessage(errorBody, "Lỗi khi thêm item!");
                Console.WriteLine($"[AdminGacha] AddItem failed: {response.StatusCode} - {errorBody}");
            }
            else
            {
                Message = "Thêm item thành công!";
            }

            return RedirectToPage();
        }

        // ── GET: All Items for Picker (JSON) ──────────────────────────
        public async Task<IActionResult> OnGetAllItemsAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.GetAsync("api/admin/gacha/items-available");

            if (!response.IsSuccessStatusCode)
                return new JsonResult(new { success = false, data = Array.Empty<object>() });

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        // ── POST: Remove Item from Banner ─────────────────────────────
        public async Task<IActionResult> OnPostRemoveItemAsync(Guid bannerId, Guid itemId)
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.DeleteAsync(
                $"api/admin/gacha/banners/{bannerId}/items/{itemId}");

            IsSuccess = response.IsSuccessStatusCode;
            Message = IsSuccess ? "Xóa item thành công!" : "Lỗi khi xóa item!";
            return RedirectToPage();
        }

        // ── Helper ────────────────────────────────────────────────────
        private static string TryExtractMessage(string json, string fallback)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg) ||
                    doc.RootElement.TryGetProperty("Message", out msg))
                    return msg.GetString() ?? fallback;
            }
            catch { }
            return fallback;
        }

        // ── DTOs ──────────────────────────────────────────────────────
        public class GachaBannerDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? BannerImagePath { get; set; }
            public int CostPerSinglePull { get; set; }
            public int CostPerMultiPull { get; set; }
            public int PityThreshold { get; set; }
            public int HardPityThreshold { get; set; }
            public bool IsActive { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<GachaBannerItemDto> AllItems { get; set; } = new();
        }

        public class GachaBannerItemDto
        {
            public Guid ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string? ItemCategory { get; set; }
            public string? ImagePath { get; set; }
            public int StarRating { get; set; }
            public double DropRate { get; set; }
            public bool IsFeatured { get; set; }
        }

        public class AddGachaItemRequest
        {
            public Guid ItemId { get; set; }
            public int StarRating { get; set; }
            public double DropRate { get; set; }
            public string ItemCategory { get; set; } = string.Empty;
            public bool IsFeatured { get; set; }
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}