using Common.Enums;

namespace Common.Models.SignalR
{
    public class NewMessageAddedModel : SignalRModelBase<NewMessageAddedModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.NewMessageAddedClient;

        public int RecipientId { get; set; }
        public string Text { get; set; } = null!;
    }
}
