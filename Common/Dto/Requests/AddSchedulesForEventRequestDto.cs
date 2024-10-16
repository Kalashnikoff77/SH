namespace Common.Dto.Requests
{
    public class AddSchedulesForEventRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/AddSchedulesForEvent";

        public List<SchedulesForEventsDto> Schedules { get; set; } = null!;
    }
}
