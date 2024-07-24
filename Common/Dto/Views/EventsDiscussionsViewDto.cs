namespace Common.Dto.Views
{
    public class EventsDiscussionsViewDto : EventsDiscussionsDto
    {
        public AccountsViewDto? Sender { get; set; }
        public AccountsViewDto? Recipient { get; set; }
    }
}
