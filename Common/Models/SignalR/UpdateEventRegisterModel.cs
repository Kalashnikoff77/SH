using Common.Dto.Views;
using Common.Enums;

namespace Common.Models.SignalR
{
    public class UpdateEventRegisterModel : SignalRModelBase<UpdateEventRegisterModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateEventRegisterClient;

        public List<EventsViewDto>? Events { get; set; } = null!;
    }
}
