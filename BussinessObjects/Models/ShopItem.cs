using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class ShopItem
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        [Required]
        public string Category { get; set; } = string.Empty; // e.g., "Currency", "Item", "Bundle"

        [MaxLength(50)]
        public string? ItemType { get; set; } // e.g., "PremiumCurrency", "RegularCurrency", "Equipment", "Consumable"

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        // Price information
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        [Display(Name = "Currency Amount to get when buying")]
        public decimal? CurrencyAmount { get; set; } // If this is a currency package

        [Display(Name = "Is using premium currency")]
        public bool IsUsingPremiumCurrency { get; set; } = false;

        // For item bundles: quantity of items included
        [Range(1, int.MaxValue)]
        [Display(Name = "Item Quantity")]
        public int? ItemQuantity { get; set; }

        // Foreign key to Item (if this shop item is selling a specific game item)
        public Guid? ItemId { get; set; }
        public Item? Item { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        // Collections for purchase history
        public ICollection<ShopPurchase> ShopPurchases { get; set; } = new List<ShopPurchase>();
    }
}
