using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class GameNews
    {
        public Guid Id { get; set; }

        [MaxLength(500)]
        [Required]
        public string Title { get; set; }

        public string BannerPath { get; set; } = null;

        [MaxLength(500)]
        [Required]
        public string Description { get; set; }

        public string? Rarity { get; set; }

        [MaxLength(10000)]
        [Required]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Publish Date")]
        public DateTime PublishDate { get; set; } = DateTime.Now;
    }
}