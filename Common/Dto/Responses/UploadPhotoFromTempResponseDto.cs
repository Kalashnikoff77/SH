namespace Common.Dto.Responses
{
    public class UploadPhotoFromTempResponseDto : ResponseDtoBase
    {
        public PhotosForAccountsDto NewPhoto { get; set; } = null!;
    }
}
