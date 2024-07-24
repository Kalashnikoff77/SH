using Common.Enums;

namespace Common.Models.SignalR
{
    public class UpdateMessagesCountModel : SignalRModelBase<UpdateMessagesCountModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateMessagesCountClient;
    }
}
