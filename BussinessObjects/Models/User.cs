using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class User : IdentityUser<Guid>
    {
        [MaxLength(50)]
        [Required]
        public override string UserName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly? UserDOB { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, ErrorMessage = "The phone number must be at most 11 digits.")]
        [RegularExpression(@"^\d{1,11}$", ErrorMessage = "The phone number must only contain digits")]
        public override string? PhoneNumber { get; set; }

        [Display(Name = "Gender")]
        public byte Gender { get; set; } // 0: male, 1: female, 3: other

        [ValidateNever]
        public string? UserAvatar { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public bool IsBanned { get; set; }

        // --- CÁC TRƯỜNG BỔ SUNG THEO ERD ---

        [Display(Name = "Currency Amount")]
        public decimal CurrencyAmount { get; set; }

        [Display(Name = "Pity Counter")]
        public int PityCounter { get; set; }


        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

        public ICollection<GachaHistory> GachaHistories { get; set; } = new List<GachaHistory>();

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}