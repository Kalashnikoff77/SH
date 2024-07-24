using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities
{
    public class EventsEntity : EntityBase
    {
        [Required]
        public Guid Guid { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public int AdminId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int RegionId { get; set; }

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public short? MaxMen { get; set; }
        [Required]
        public short? MaxWomen { get; set; }
        [Required]
        public short? MaxPairs { get; set; }

        [Required]
        public int? CostMan { get; set; }
        [Required]
        public int? CostWoman { get; set; }
        [Required]
        public int? CostPair { get; set; }

        public bool IsDeleted { get; set; }
    }
}
