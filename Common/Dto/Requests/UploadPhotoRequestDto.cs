namespace Common.Dto.Requests
{
    public class UploadPhotoRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/Upload";

        public List<string> PhotoNames { get; set; } = null!;
    }
}
