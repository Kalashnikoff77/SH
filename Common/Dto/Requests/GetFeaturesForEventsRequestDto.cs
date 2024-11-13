namespace Common.Dto.Requests
{
    public class GetFeaturesForEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/GetFeaturesForEvents";

        public bool IsActualEvents { get; set; }
    }
}
