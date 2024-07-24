using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities
{
    public class AccountsPhotosEntity : EntityBase
    {
        [Required]
        public Guid Guid { get; set; }

        [Required]
        public string? Comment { get; set; }

        [Required]
        public bool IsAvatar { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public int AccountId { get; set; }
    }
}
