using Common.Enums;

namespace Common.Models.SignalR
{
    public class OnEventDiscussionAddedResponse : SignalRModelBase<OnEventDiscussionAddedResponse>
    {
        public int Id { get; set; }

        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.OnEventDiscussionAddedClient;
    }
}
