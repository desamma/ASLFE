using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class GachaBanner
    {
        public Guid Id { get; set; }

        public string? BannerPath { get; set; } = null;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime BannerStartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime BannerEndDate { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Roll cost cannot be negative.")]
        public int RollCost { get; set; }

        public ICollection<GachaItem> GachaItems { get; set; } = new List<GachaItem>();
    }
}