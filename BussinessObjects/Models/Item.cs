using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class Item
    {
        public Guid Id { get; set; }

        [Required]
        public string DictionaryKey { get; set; } = string.Empty;

        [MaxLength(50)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        [Required]
        public string Type { get; set; } = string.Empty;

        [MaxLength(50)]
        [Required]
        public string Rarity { get; set; } = string.Empty;

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public bool IsGachaOnly { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public List<string> StatsLines { get; set; } = new List<string>();

        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

        
    }
}