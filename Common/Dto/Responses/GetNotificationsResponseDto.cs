using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetNotificationsResponseDto : ResponseDtoBase
    {
        public AccountsViewDto Sender { get; set; } = null!;

        public List<NotificationsViewDto>? Notifications { get; set; }
    }
}
