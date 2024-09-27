using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetSchedulesResponseDto : ResponseDtoBase
    {
        public SchedulesForEventsViewDto? Schedule { get; set; }

        public List<SchedulesForEventsViewDto>? Schedules { get; set; }
    }
}
