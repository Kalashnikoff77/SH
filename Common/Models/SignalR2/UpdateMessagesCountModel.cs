using Common.Enums;

namespace Common.Models.SignalR2
{
    public class UpdateMessagesCountModel : SignalRModelBase<UpdateMessagesCountModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateMessagesCountClient;
    }
}
