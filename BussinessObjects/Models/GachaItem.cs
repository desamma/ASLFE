using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class GachaItem
    {
        public Guid Id { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Rate must be between 0 and 100.")]
        public double GachaRate { get; set; }

        public bool IsFeaturedItem { get; set; }

        [Required]
        public Guid GachaBannerId { get; set; }
        public GachaBanner? GachaBanner { get; set; }

        [Required]
        public Guid ItemId { get; set; }
        public Item? Item { get; set; }

        public ICollection<GachaHistory> GachaHistories { get; set; } = new List<GachaHistory>();
    }
}