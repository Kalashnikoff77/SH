namespace Common.Dto
{
    public class AccountsPhotosDto : DtoBase
    {
        public Guid Guid { get; set; }

        public string? Comment { get; set; }

        public bool IsAvatar { get; set; }
    }
}
