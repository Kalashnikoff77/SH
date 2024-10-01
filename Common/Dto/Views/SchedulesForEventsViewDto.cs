namespace Common.Dto.Views
{
    public class SchedulesForEventsViewDto : SchedulesForEventsDto
    {
        public EventsViewDto? Event { get; set; }

        public List<FeaturesDto>? Features { get; set; }

        public List<SchedulesForAccountsViewDto>? RegisteredAccounts { get; set; }

        public int NumberOfDiscussions { get; set; }
    }
}
