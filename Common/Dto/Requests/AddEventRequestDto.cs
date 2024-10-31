using Common.Dto.Views;

namespace Common.Dto.Requests
{
    public class AddEventRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Add";

        public EventsViewDto Event { get; set; } = null!;
    }
}
