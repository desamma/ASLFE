using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BussinessObjects.Models
{
    public class UserItem
    {
        public Guid UserId { get; set; }
        [ValidateNever]
        public virtual User User { get; set; }

        public Guid ItemId { get; set; }
        [ValidateNever]
        public virtual Item Item { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityDelivered { get; set; } = 0;

        public bool IsDeliveredToGame { get; set; } = false;
        public DateTime? DeliveredToGameAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
