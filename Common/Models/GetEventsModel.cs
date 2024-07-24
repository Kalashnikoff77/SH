namespace Common.Models
{
    public class GetEventsModel : ModelBase
    {
        public int? AccountId { get; set; }

        public bool IsPhotosIncluded { get; set; }
    }
}
