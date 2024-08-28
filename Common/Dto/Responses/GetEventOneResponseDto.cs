using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetEventOneResponseDto : ResponseDtoBase
    {
        public SchedulesForEventsViewDto Event { get; set; } = null!;
    }
}
