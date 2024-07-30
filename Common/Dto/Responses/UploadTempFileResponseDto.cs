namespace Common.Dto.Responses
{
    public class UploadTempFileResponseDto : ResponseDtoBase
    {
        public string previewFileName { get; set; } = null!;
        public string originalFileName { get; set; } = null!;
    }
}
