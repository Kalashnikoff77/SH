namespace Common.Dto.Responses
{
    public class UploadPhotoToTempResponseDto : ResponseDtoBase
    {
        public PhotosForEventsDto NewPhoto { get; set; } = null!;
    }
}
