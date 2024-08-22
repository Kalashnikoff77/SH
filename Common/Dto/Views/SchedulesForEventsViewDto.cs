namespace Common.Dto.Views
{
    public class SchedulesForEventsViewDto : EventsDto
    {
        public int SE_Id { get; set; }
        public string? SE_Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CostMan { get; set; }
        public int CostWoman { get; set; }
        public int CostPair { get; set; }
        public AccountsViewDto? Admin { get; set; } = null!;
        public CountriesDto? Country { get; set; } = null!;
        public PhotosForEventsDto? Avatar { get; set; } = null!;
        public List<PhotosForEventsDto>? Photos { get; set; }
        public int NumOfDiscussions { get; set; }
    }
}
