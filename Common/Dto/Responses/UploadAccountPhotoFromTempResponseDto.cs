namespace Common.Dto.Responses
{
    public class UploadAccountPhotoFromTempResponseDto : ResponseDtoBase
    {
        public PhotosForAccountsDto NewPhoto { get; set; } = null!;
    }
}
