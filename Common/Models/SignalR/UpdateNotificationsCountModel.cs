using Common.Enums;

namespace Common.Models.SignalR
{
    public class UpdateNotificationsCountModel : SignalRModelBase<UpdateNotificationsCountModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateNotificationsCountClient;
    }
}
