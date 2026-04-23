using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class BugReport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string Steps { get; set; }

        [MaxLength(200)]
        public string ExpectedBehavior { get; set; }

        [MaxLength(200)]
        public string ActualBehavior { get; set; }

        [MaxLength(50)]
        public string Severity { get; set; } // Low, Medium, High, Critical

        [MaxLength(50)]
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        [MaxLength(500)]
        public string AdminNotes { get; set; }
    }
}
