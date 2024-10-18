using Common.Dto.Views;

namespace Common.Dto.Requests
{
    public class UpdateEventRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Update";

        public EventsViewDto Event { get; set; } = null!;
    }
}
