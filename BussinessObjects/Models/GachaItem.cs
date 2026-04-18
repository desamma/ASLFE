using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class GachaItem
    {
        public Guid Id { get; set; }

        public Guid GachaBannerId { get; set; }
        [ForeignKey("GachaBannerId")]
        public GachaBanner? GachaBanner { get; set; }

        public Guid ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item? Item { get; set; }

        // Tỷ lệ rơi %
        [Range(0.001, 100.0)]
        public double DropRate { get; set; }

        // Số sao: 3, 4, 5 (theo report dùng Roman numeral IV=4, V=5)
        [Range(1, 5)]
        public int StarRating { get; set; }

        // "Weapon" hoặc "Character"
        [MaxLength(50)]
        public string ItemCategory { get; set; } = string.Empty;

        // Rate-up (featured) item hay không
        public bool IsFeatured { get; set; } = false;
    }
}