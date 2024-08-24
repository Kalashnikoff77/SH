namespace Common.Dto.Requests
{
    public class GetEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Get";

        /// <summary>
        /// Встречи какого организатора выводить
        /// </summary>
        public int? AdminId { get; set; }

        /// <summary>
        /// Встречи из каких регионов выводить
        /// </summary>
        public List<int>? RegionsIds { get; set; }

        /// <summary>
        /// Встречи с какими тегами выводить
        /// </summary>
        public List<int>? FeaturesIds { get; set; } = null;

        public bool IsPhotosIncluded { get; set; }
    }
}
