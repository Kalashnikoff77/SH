using Common.Dto.Views;
using Common.Enums;

namespace Common.Models.SignalR
{
    public class UpdateEventSubscribeOrRegisterModel : SignalRModelBase<UpdateEventSubscribeOrRegisterModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateEventSubscribeOrRegisterClient;

        public List<EventsViewDto>? Events { get; set; } = null!;
    }
}
