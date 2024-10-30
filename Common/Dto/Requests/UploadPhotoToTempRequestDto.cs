using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace Common.Dto.Requests
{
    public class UploadPhotoToTempRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/UploadPhotoToTemp";

        /// <summary>
        /// Id аккаунта или события, к которому принадлежит фото
        /// </summary>
        public int Id { get; set; }

        public byte[]? File { get; set; }
    }
}
