namespace Common.Dto.Responses
{
    public class UploadEventPhotoFromTempResponseDto : ResponseDtoBase
    {
        public PhotosForEventsDto NewPhoto { get; set; } = null!;
    }
}
