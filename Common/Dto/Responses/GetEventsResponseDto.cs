using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetEventsResponseDto : ResponseDtoBase
    {
        public List<EventsViewDto> Events { get; set; } = new List<EventsViewDto>();
    }
}
