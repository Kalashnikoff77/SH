namespace Common.Dto.Requests
{
    public class UpdateEventRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Update";

        public EventsDto Event { get; set; } = null!;
        public List<SchedulesForEventsDto> Schedules { get; set; } = null!;
    }
}
