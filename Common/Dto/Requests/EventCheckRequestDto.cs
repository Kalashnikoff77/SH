namespace Common.Dto.Requests
{
    public class EventCheckRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Check";

        // Если передан EventId, значит, обновление этого мероприятия, иначе добавление
        public int? EventId { get; set; }

        public string? EventName { get; set; }
    }
}
