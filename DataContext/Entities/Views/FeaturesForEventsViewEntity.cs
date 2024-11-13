namespace DataContext.Entities.Views
{
    public class FeaturesForEventsViewEntity : FeaturesEntity
    {
        public DateTime EndDate { get; set; }

        public int AdminId { get; set; }

        public int RegionId { get; set; }

        // TODO Можно удалить (OK)
        public int NumberOfEvents { get; set; }
    }
}
