using Common.Dto;
using Common.Enums;

namespace Common.Models.SignalR
{
    public class OnAvatarChangedResponse : SignalRModelBase<OnAvatarChangedResponse>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.OnAvatarChangedClient;

        public PhotosForAccountsDto NewAvatar { get; set; } = null!;
    }
}
