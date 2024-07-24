namespace Common.Dto.Requests
{
    public class GetEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Get";

        /// <summary>
        /// Id пользователя, чьи события выводить
        /// </summary>
        public int? AccountId { get; set; }

        public bool IsPhotosIncluded { get; set; }
    }
}
