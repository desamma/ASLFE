using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class NPC
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(50)]
        [Required]
        public string NPCType { get; set; } = string.Empty; // e.g., "Merchant", "QuestGiver", "DialogOnly", "Companion"

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UpdatedDate { get; set; }

        [NotMapped]
        public string DisplayType
        {
            get
            {
                return NPCType switch
                {
                    "DialogOnly" => "Dialog Only",
                    _ => NPCType
                };
            }
        }
    }
}
