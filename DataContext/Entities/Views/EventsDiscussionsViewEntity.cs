using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities.Views
{
    public class EventsDiscussionsViewEntity : EventsDiscussionsEntity
    {
        [Required]
        public string? Sender { get; set; }
        [Required]
        public string? Recipient { get; set; }
    }
}
