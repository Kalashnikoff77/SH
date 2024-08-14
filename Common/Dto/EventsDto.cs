namespace Common.Dto
{
    public class EventsDto : DtoBase
    {
        public Guid Guid { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int RegionId { get; set; }

        public string Address { get; set; } = null!;

        public short? MaxMen { get; set; }
        public short? MaxWomen { get; set; }
        public short? MaxPairs { get; set; }

        public int? CostMan { get; set; }
        public int? CostWoman { get; set; }
        public int? CostPair { get; set; }
    }
}
