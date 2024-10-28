using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities
{
    public class PhotosForEventsEntity : EntityBase
    {
        [Required]
        public Guid Guid { get; set; }

        [Required]
        public string? Comment { get; set; }

        [Required]
        public bool IsAvatar { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public int EventId { get; set; }
    }
}
