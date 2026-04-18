using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class GachaHistory
    {
        public Guid Id { get; set; }

        // ✅ ADD THIS - User Reference
        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public Guid GachaBannerId { get; set; }
        [ForeignKey("GachaBannerId")]
        public virtual GachaBanner? GachaBanner { get; set; }

        [Required]
        public Guid ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        [Range(1, 5)]
        public int StarRating { get; set; }

        public bool IsFeatured { get; set; }

        /// <summary>Pull này do pity kích hoạt</summary>
        public bool WasPityTriggered { get; set; }

        /// <summary>Thứ tự trong batch (1–10)</summary>
        public int PullNumberInSession { get; set; }

        /// <summary>Giá trị pity lúc pull</summary>
        public int PityCounterSnapshot { get; set; }

        /// <summary>"SinglePull" | "MultiPull"</summary>
        [MaxLength(20)]
        public string PullType { get; set; } = string.Empty;

        public int GemsCost { get; set; }

        public DateTime PulledAt { get; set; } = DateTime.Now;
    }
}