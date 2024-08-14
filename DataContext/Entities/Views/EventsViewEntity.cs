namespace DataContext.Entities.Views
{
    public class EventsViewEntity : EventsEntity
    {
        public string Admin { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string Avatar { get; set; } = null!;

        public string? Photos { get; set; }

        public string? Schedule { get; set; }

        public int? NumOfSubscribers { get; set; }
        public int? NumOfRegisters { get; set; }
        public int? NumOfDiscussions { get; set; }

        // Для AccountsEventsView
        public bool IsSubscribed { get; set; }
        public bool IsRegistered { get; set; }
    }
}
