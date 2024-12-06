using Common.Dto.Views;
using Common.Enums;

namespace Common.Models.SignalR
{
    public class OnScheduleChangedResponse : SignalRModelBase<OnScheduleChangedResponse>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.OnScheduleChangedClient;

        public SchedulesForEventsViewDto? UpdatedSchedule { get; set; }
    }
}
