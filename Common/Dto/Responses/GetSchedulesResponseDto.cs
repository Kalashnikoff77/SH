using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetSchedulesResponseDto : ResponseDtoBase
    {
        public SchedulesForEventsViewDto? Event { get; set; }

        public List<SchedulesForEventsViewDto>? Events { get; set; }
    }
}
