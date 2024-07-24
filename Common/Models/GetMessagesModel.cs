namespace Common.Models
{
    /// <summary>
    /// MessagesController
    /// </summary>
    public class GetMessagesModel : ModelBase
    {
        public int RecipientId { get; set; }

        public int? GetPreviousFromId { get; set; }
        public int? GetNextAfterId { get; set; }

        public bool MarkAsRead { get; set; } = false;

        public int Take { get; set; } = StaticData.MESSAGES_PER_BLOCK;
    }
}
