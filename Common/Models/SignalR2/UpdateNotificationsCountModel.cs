using Common.Enums;

namespace Common.Models.SignalR2
{
    public class UpdateNotificationsCountModel : SignalRModelBase<UpdateNotificationsCountModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateNotificationsCountClient;
    }
}
