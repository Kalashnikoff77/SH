namespace Common.Dto.Requests
{
    public class GetEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Get";

        /// <summary>
        /// Получить одну запись с указанным ScheduleId
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Встречи каких организаторов выводить
        /// </summary>
        public IEnumerable<int>? AdminsIds { get; set; }

        /// <summary>
        /// Встречи из каких регионов выводить
        /// </summary>
        public IEnumerable<int>? RegionsIds { get; set; }

        /// <summary>
        /// Встречи с какими тегами выводить
        /// </summary>
        public IEnumerable<int>? FeaturesIds { get; set; } = null;

        public bool IsPhotosIncluded { get; set; }
    }
}
