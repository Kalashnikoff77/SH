namespace Common.Dto.Requests
{
    public class UploadPhotoFromTempRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/UploadFromTemp";

        /// <summary>
        /// Ссылки на имена необработанных файлов фоток в каталоге temp
        /// </summary>
        public string PhotosTempFileNames { get; set; } = null!;
    }
}
