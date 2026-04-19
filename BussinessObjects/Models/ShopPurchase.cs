using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class ShopPurchase
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public Guid ShopItemId { get; set; }
        public ShopItem? ShopItem { get; set; }

        [Range(1, int.MaxValue)]
        [Required]
        [Display(Name = "Quantity Purchased")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Payment Type")]
        public string PaymentType { get; set; } = string.Empty; // "RegularCurrency" or "PremiumCurrency"

        [Required]
        [Display(Name = "Amount Paid")]
        public int AmountPaid { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
    }
}
