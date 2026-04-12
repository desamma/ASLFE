using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class GachaHistory
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public Guid GachaItemId { get; set; }
        public GachaItem? GachaItem { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsSuccess { get; set; }

        [Required]
        public decimal NewUserBalance { get; set; }
    }
}