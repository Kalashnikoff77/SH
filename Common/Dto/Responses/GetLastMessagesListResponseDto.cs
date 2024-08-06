using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetLastMessagesListResponseDto : ResponseDtoBase
    {
        public List<LastMessagesListViewDto>? LastMessagesList { get; set; }

        public int Count { get; set; }
    }
}
