namespace Common.Dto
{
    public class SchedulesForEventsDto : DtoBase
    {
        public int EventId { get; set; }
        public Guid Guid { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RegionId { get; set; }
        public string Address { get; set; } = null!;
        public int MaxMen { get; set; }
        public int MaxWomen { get; set; }
        public int MaxPairs { get; set; }
        public int CostMan { get; set; }
        public int CostWoman { get; set; }
        public int CostPair { get; set; }

        public List<FeaturesDto> Features { get; set; } = null!;
    }
}
