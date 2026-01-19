using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class Item
    {
        public Guid Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        [MaxLength(500)]
        [Required]
        public string Description { get; set; }

        [MaxLength(50)]
        [Required]
        public string Type { get; set; }

        [MaxLength(50)]
        [Required]
        public string Rarity { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();
    }
}