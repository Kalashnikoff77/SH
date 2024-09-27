using Common.Enums;

namespace Common.Models.SignalR
{
    public class OnScheduleChangedResponse : SignalRModelBase<OnScheduleChangedResponse>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.OnScheduleChangedClient;

        public int? EventId { get; set; }
        public int ScheduleId { get; set; }
    }
}
