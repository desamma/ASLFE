using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Mã đơn hàng gửi lên PayOS — phải là long, unique, dương
        /// Sinh bằng: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        /// </summary>
        [Required]
        public long OrderCode { get; set; }

        /// <summary>Tên gói nạp, ví dụ: "Gói 110 VP"</summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>Loại giao dịch: TopUp | ShopPurchase</summary>
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "TopUp";

        /// <summary>Số tiền VND thanh toán thực tế</summary>
        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }

        /// <summary>Số VP (currency) user nhận được sau khi thanh toán</summary>
        [Required]
        public int CurrencyAwarded { get; set; }

        /// <summary>Pending | Paid | Cancelled | Failed</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>URL trang thanh toán PayOS trả về — redirect user đến đây</summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>QR code URL (optional)</summary>
        public string? QrCodeUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // FK tới User
        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}