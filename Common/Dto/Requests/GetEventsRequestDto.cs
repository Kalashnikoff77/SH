namespace Common.Dto.Requests
{
    public class GetEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Get";

        /// <summary>
        /// Id пользователя, чьи события выводить
        /// </summary>
        public int? AccountId { get; set; }

        public List<int> FeaturesIds { get; set; } = new List<int>(5);

        public bool IsPhotosIncluded { get; set; }
    }
}
