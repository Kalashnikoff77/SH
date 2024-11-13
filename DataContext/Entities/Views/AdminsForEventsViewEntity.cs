namespace DataContext.Entities.Views
{
    public class AdminsForEventsViewEntity : EntityBase
    {
        public string Name { get; set; } = null!;

        public DateTime EndDate { get; set; }

        public int RegionId { get; set; }

        // TODO REMOVE (OK)
        public int NumberOfEvents { get; set; }
    }
}
