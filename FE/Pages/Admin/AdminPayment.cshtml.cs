using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FE.Pages.Admin
{
    [Authorize(Roles = "admin")]
    public class AdminPaymentModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminPaymentModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string? Tab { get; set; } 

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? OrderCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Quantity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ItemType { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public List<ShopPurchaseDto> ShopPurchases { get; set; } = new List<ShopPurchaseDto>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");

            try
            {
                // 1. GHÉP URL VÀ LẤY DANH SÁCH NẠP TIỀN PAYOS
                var txQueryParams = new List<string>();
                if (!string.IsNullOrEmpty(StatusFilter)) txQueryParams.Add($"status={Uri.EscapeDataString(StatusFilter)}");
                if (!string.IsNullOrEmpty(OrderCode)) txQueryParams.Add($"orderCode={Uri.EscapeDataString(OrderCode)}");

                var txUrl = "api/admin/payments/transactions";
                if (txQueryParams.Any()) txUrl += "?" + string.Join("&", txQueryParams);

                var txResponse = await client.GetAsync(txUrl);
                if (txResponse.IsSuccessStatusCode)
                {
                    var result = await txResponse.Content.ReadFromJsonAsync<ApiResponse<List<TransactionDto>>>();
                    if (result != null && result.Success) Transactions = result.Data ?? new List<TransactionDto>();
                }

                // 2. GHÉP URL VÀ LẤY DANH SÁCH MUA ĐỒ TRONG SHOP
                var shopQueryParams = new List<string>();
                if (!string.IsNullOrEmpty(SearchName)) shopQueryParams.Add($"searchName={Uri.EscapeDataString(SearchName)}");
                if (!string.IsNullOrEmpty(ItemType)) shopQueryParams.Add($"itemType={Uri.EscapeDataString(ItemType)}");
                if (Quantity.HasValue) shopQueryParams.Add($"quantity={Quantity.Value}");

                var shopUrl = "api/admin/payments/shop-purchases";
                if (shopQueryParams.Any()) shopUrl += "?" + string.Join("&", shopQueryParams);

                var shopResponse = await client.GetAsync(shopUrl);
                if (shopResponse.IsSuccessStatusCode)
                {
                    var result = await shopResponse.Content.ReadFromJsonAsync<ApiResponse<List<ShopPurchaseDto>>>();
                    if (result != null && result.Success) ShopPurchases = result.Data ?? new List<ShopPurchaseDto>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối đến máy chủ: " + ex.Message;
            }
        }
    }

    public class TransactionDto
    {
        public long OrderCode { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // Tên gói
        public int Amount { get; set; } // Tiền nạp (VND)
        public int CurrencyAwarded { get; set; } // VP nhận được
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ShopPurchaseDto
    {
        public string UserName { get; set; } = string.Empty;
        public string ShopItemName { get; set; } = string.Empty;

        // Đã thêm Rarity để khớp với giao diện HTML
        public string Rarity { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public int AmountPaid { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}