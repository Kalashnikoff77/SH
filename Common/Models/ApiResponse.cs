using Common.Dto.Responses;
using System.Net;

namespace Common.Models
{
    public class ApiResponse<TResponseDto> where TResponseDto : ResponseDtoBase, new()
    {
        public HttpStatusCode StatusCode { get; set; }

        public TResponseDto Response { get; set; } = new TResponseDto();
    }
}
