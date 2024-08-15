namespace Common.Dto.Views
{
    public class EventsViewDto : EventsDto
    {
        public DateTime NearestDate { get; set; }

        public AccountsViewDto? Admin { get; set; }

        public CountriesDto? Country { get; set; }

        public EventsPhotosDto? Avatar { get; set; }

        public List<EventsPhotosDto>? Photos { get; set; }

        public List<EventsSchedulesDto>? Schedule { get; set; }

        public int? NumOfRegisters { get; set; }
        public int? NumOfDiscussions { get; set; }

        // Для AccountsEventsView
        public bool IsRegistered { get; set; }
    }
}
