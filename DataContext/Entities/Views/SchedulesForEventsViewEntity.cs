using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities.Views
{
    public class SchedulesForEventsViewEntity : EventsEntity
    {
        [Required]
        public int SE_Id { get; set; }

        [Required]
        public string? SE_Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int CostMan { get; set; }

        [Required]
        public int CostWoman { get; set; }

        [Required]
        public int CostPair { get; set; }

        [Required]
        public string? Admin { get; set; }

        [Required]
        public string? Country { get; set; }

        [Required]
        public string? Avatar { get; set; }

        public string? Photos { get; set; }

        [Required]
        public int NumOfDiscussions { get; set; }
    }
}
