using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetScheduleOneResponseDto : ResponseDtoBase
    {
        public SchedulesForEventsViewDto Event { get; set; } = null!;
    }
}
