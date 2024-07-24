namespace Common.Models
{
    public class AddMessageModel : ModelBase
    {
        public int RecipientId { get; set; }

        public DateTime? ReadDate { get; set; }

        public string Text { get; set; } = null!;
    }
}
