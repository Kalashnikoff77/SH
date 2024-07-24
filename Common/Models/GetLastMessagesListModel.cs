namespace Common.Models
{
    public class GetLastMessagesListModel : ModelBase
    {
        public int RecipientId { get; set; }

        public int Take { get; set; } = 20;
    }
}
