using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class GachaBanner
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // "Character Event Wish"

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string BannerImagePath { get; set; } = string.Empty;

        // Chi phí (theo report: 100 Gems x1, 1000 Gems x10)
        public int CostPerSinglePull { get; set; } = 100;
        public int CostPerMultiPull { get; set; } = 1000;   // x10
        public int MultiPullCount { get; set; } = 10;

        // Pity: guarantee 4-star mỗi x10 (theo report)
        public int PityThreshold { get; set; } = 10;        // 10 pulls → guaranteed 4★+
        public int HardPityThreshold { get; set; } = 90;    // guaranteed 5★ (legendary)

        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<GachaItem> GachaItems { get; set; } = new List<GachaItem>();
    }
}