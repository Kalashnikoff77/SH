namespace Common.Dto.Requests
{
    public class UpdatePhotoRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/Update";

        public Guid Guid { get; set; }

        public bool IsAvatarChanging { get; set; }

        public bool IsCommentChanging { get; set; }

        public bool IsDeleting { get; set; }

        public string? Comment { get; set; }
    }
}
