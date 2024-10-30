namespace Common.Dto.Requests
{
    public class UploadPhotoToTempRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/UploadPhotoToTemp";

        /// <summary>
        /// Id аккаунта, к которому принадлежит фото
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Id события, к которому принадлежит фото
        /// </summary>
        public int? EventId { get; set; }

        /// <summary>
        /// Само фото
        /// </summary>
        public byte[]? File { get; set; }
    }
}
