using Common.Dto.Views;
using Common.Enums;

namespace Common.Models.SignalR
{
    public class OnEventDiscussionAddedResponse : SignalRModelBase<OnEventDiscussionAddedResponse>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.OnEventDiscussionAddedClient;

        public SchedulesForEventsViewDto ScheduleForEventViewDto { get; set; } = null!;
    }
}
