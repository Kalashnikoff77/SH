using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class MarkMessageAsReadResponseDto : ResponseDtoBase
    {
        public LastMessagesListViewDto? UpdatedMessage { get; set; }
    }
}
