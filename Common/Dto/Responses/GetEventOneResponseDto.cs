using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetEventOneResponseDto : ResponseDtoBase
    {
        public EventsViewDto Event { get; set; } = null!;
    }
}
