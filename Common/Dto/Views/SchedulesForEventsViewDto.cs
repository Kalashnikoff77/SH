namespace Common.Dto.Views
{
    public class SchedulesForEventsViewDto : SchedulesForEventsDto
    {
        public EventsViewDto? Event { get; set; }

        public List<FeaturesDto>? Features { get; set; }

        public List<AccountsForSchedulesDto>? RegisteredAccounts { get; set; }
    }
}
