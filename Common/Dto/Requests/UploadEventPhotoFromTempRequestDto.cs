namespace Common.Dto.Requests
{
    public class UploadEventPhotoFromTempRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/UploadEventFromTemp";

        /// <summary>
        /// Id события, к которому принадлежит фото
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Ссылка на имя необработанного файла фоток в каталоге temp
        /// </summary>
        public string PhotosTempFileNames { get; set; } = null!;
    }
}
