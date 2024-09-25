namespace Common.Dto.Requests
{
    public class UploadPhotosFromTempRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/UploadFromTemp";

        /// <summary>
        /// Ссылки на имена необработанных файлов фоток в каталоге temp
        /// </summary>
        public List<string> PhotosTempFileNames { get; set; } = null!;
    }
}
