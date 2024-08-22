namespace Common.Models
{
    public class GetEventsModel : ModelBase
    {
        /// <summary>
        /// Встречи какого организатора выводить
        /// </summary>
        public int? AdminId { get; set; }

        /// <summary>
        /// Встречи с какими тегами выводить
        /// </summary>
        public List<int>? FeaturesIds { get; set; } = null;

        public bool IsPhotosIncluded { get; set; }
    }
}
