using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class GameNews
    {
        public Guid Id { get; set; }

        [MaxLength(500)]
        [Required]
        public string Title { get; set; }

        [Required]
        public string BannerPath { get; set; }

        [MaxLength(500)]
        [Required]
        public string Description { get; set; }

        [MaxLength(10000)]
        [Required]
        public string Content { get; set; }
    }
}