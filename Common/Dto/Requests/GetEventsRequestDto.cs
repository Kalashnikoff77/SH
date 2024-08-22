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
        /// Встречи с какими тегами выводить
        /// </summary>
        public List<int>? FeaturesIds { get; set; } = null;
    }
}
