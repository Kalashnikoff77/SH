namespace Common.Dto
{
    public class PhotosForAccountsDto : DtoBase
    {
        public Guid Guid { get; set; }

        public string? Comment { get; set; }

        public bool IsAvatar { get; set; }
    }
}
