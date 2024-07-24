using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetEventDiscussionsResponseDto : ResponseDtoBase
    {
        public int NumOfDiscussions { get; set; }
        public List<EventsDiscussionsViewDto> Discussions { get; set; } = new List<EventsDiscussionsViewDto>();
    }
}
