using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities
{
    public class PhotosForAccountsEntity : EntityBase
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
