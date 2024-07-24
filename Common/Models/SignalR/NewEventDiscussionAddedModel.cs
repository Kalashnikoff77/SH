using Common.Enums;

namespace Common.Models.SignalR
{
    /// <summary>
    /// Клиент: NewEventDiscussionAddedClient
    /// </summary>
    public class NewEventDiscussionAddedModel : SignalRModelBase<NewEventDiscussionAddedModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.NewEventDiscussionAddedClient;

        /// <summary>
        /// Id события
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Добавленный Id обсуждения в событии
        /// </summary>
        public int NewDiscussionId { get; set; }
    }
}
