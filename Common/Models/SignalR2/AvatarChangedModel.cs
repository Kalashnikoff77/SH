using Common.Enums;

namespace Common.Models.SignalR2
{
    public class AvatarChangedModel : SignalRModelBase<AvatarChangedModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.AvatarChangedClient;

        public int AccountId { get; set; }
        public bool IsAvatar { get; set; }
        public Guid Guid { get; set; }
        public string? Comment { get; set; }
    }
}
