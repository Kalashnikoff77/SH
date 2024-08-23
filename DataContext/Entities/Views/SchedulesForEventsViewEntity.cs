using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities.Views
{
    public class SchedulesForEventsViewEntity : SchedulesForEventsEntity
    {
        [Required]
        public string? Event { get; set; }
        [Required]
        public string? Features { get; set; }
    }
}
