using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Rarity { get; set; }

        public string? ImagePath { get; set; }

        // Khóa ngoại tới User
        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}