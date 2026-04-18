using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public string StatusFilter { get; set; }

        public string ErrorMessage { get; set; }

        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public List<ShopPurchaseDto> ShopPurchases { get; set; } = new List<ShopPurchaseDto>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("Api");

            try
            {
                // 1. Lấy danh sách nạp tiền PayOS
                var txUrl = "api/admin/payments/transactions";
                if (!string.IsNullOrEmpty(StatusFilter))
                {
                    txUrl += $"?status={Uri.EscapeDataString(StatusFilter)}";
                }

                var txResponse = await client.GetAsync(txUrl);
                if (txResponse.IsSuccessStatusCode)
                {
                    var result = await txResponse.Content.ReadFromJsonAsync<ApiResponse<List<TransactionDto>>>();
                    if (result != null && result.Success) Transactions = result.Data ?? new List<TransactionDto>();
                }

                // 2. Lấy danh sách mua đồ trong Shop
                var shopResponse = await client.GetAsync("api/admin/payments/shop-purchases");
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
        public string UserName { get; set; }
        public string Name { get; set; } // Tên gói
        public int Amount { get; set; } // Tiền nạp (VND)
        public int CurrencyAwarded { get; set; } // VP nhận được
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ShopPurchaseDto
    {
        public string UserName { get; set; }
        public string ShopItemName { get; set; }
        public int Quantity { get; set; }
        public string PaymentType { get; set; }
        public int AmountPaid { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}