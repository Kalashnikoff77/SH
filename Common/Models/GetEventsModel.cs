namespace Common.Models
{
    public class GetEventsModel : ModelBase
    {
        public int? AccountId { get; set; }

        public List<int>? FeaturesIds { get; set; } = null;

        public bool IsPhotosIncluded { get; set; }
    }
}
