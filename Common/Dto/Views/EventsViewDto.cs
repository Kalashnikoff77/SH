namespace Common.Dto.Views
{
    public class EventsViewDto : EventsDto
    {
        public List<SchedulesForEventsViewDto>? Schedule { get; set; }

        public AccountsViewDto? Admin { get; set; }

        public CountriesDto? Country { get; set; }

        public PhotosForEventsDto? Avatar { get; set; }

        public List<PhotosForEventsDto>? Photos { get; set; }

        public int? NumOfDiscussions { get; set; }
    }
}
