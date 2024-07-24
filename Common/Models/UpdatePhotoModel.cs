namespace Common.Models
{
    public class UpdatePhotoModel : ModelBase
    {
        public Guid Guid { get; set; }

        public bool IsAvatar { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string? Comment { get; set; }
    }
}
