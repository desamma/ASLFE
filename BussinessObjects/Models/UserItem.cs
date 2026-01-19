using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class UserItem
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid ItemId { get; set; }
        public Item Item { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
