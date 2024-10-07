namespace Common.Dto.Requests
{
    public class EventCheckAddingRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/CheckAdding";

        public string? EventName { get; set; }
    }
}
