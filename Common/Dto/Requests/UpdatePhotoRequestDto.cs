namespace Common.Dto.Requests
{
    public class UpdatePhotoRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/Update";

        public Guid Guid { get; set; }

        public bool IsAvatar { get; set; }

        public bool IsDeleted { get; set; } = false;

        public string? Comment { get; set; }
    }
}
