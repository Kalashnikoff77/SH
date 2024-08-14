namespace Common.Models
{
    public class GetMessagesModel : ModelBase
    {
        public int RecipientId { get; set; }

        public int? GetPreviousFromId { get; set; }
        public int? GetNextAfterId { get; set; }

        public bool MarkAsRead { get; set; } = false;
    }
}
