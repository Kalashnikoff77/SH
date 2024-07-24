namespace Common.Models
{
    public class AddEventDiscussionModel : ModelBase
    {
        public int EventId { get; set; }

        /// <summary>
        /// Кому отправляется ответ
        /// </summary>
        public int? RecipientId { get; set; }

        /// <summary>
        /// На какое сообщение отправляется ответ
        /// </summary>
        public int? DiscussionId { get; set; }

        public string Text { get; set; } = null!;
    }
}
