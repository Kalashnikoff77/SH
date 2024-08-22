namespace Common.Dto.Views
{
    public class EventsViewDto : EventsDto
    {
        public DateTime NearestDate { get; set; }

        public AccountsViewDto? Admin { get; set; }

        public CountriesDto? Country { get; set; }

        public PhotosForEventsDto? Avatar { get; set; }

        public List<PhotosForEventsDto>? Photos { get; set; }

        public List<EventsForAccountsDto>? RegisteredAccounts { get; set; }

        public List<SchedulesForEventsDto>? Schedule { get; set; }

        public int NumOfDiscussions { get; set; }
    }
}
