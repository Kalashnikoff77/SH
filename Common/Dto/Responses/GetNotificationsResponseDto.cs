using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetNotificationsResponseDto : ResponseDtoBase
    {
        public List<NotificationsViewDto>? Notifications { get; set; }

        public int Count { get; set; }
    }
}
