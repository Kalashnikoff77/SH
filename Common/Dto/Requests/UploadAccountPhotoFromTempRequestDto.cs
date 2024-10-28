using Common.Enums;

namespace Common.Dto.Requests
{
    public class UploadAccountPhotoFromTempRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/UploadAccountFromTemp";

        /// <summary>
        /// Ссылка на имя необработанного файла фоток в каталоге temp
        /// </summary>
        public string PhotosTempFileNames { get; set; } = null!;
    }
}
