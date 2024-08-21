namespace DataContext.Entities
{
    public class FeaturesForSchedules : EntityBase
    {
        public int ScheduleId { get; set; }
        public int FeatureId { get; set; }
    }
}
