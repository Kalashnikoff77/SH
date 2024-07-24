using Common.Enums;

namespace Common.Models
{
    public class AddNotificationModel : ModelBase
    {
        public string Text { get; set; } = null!;

        public int RecipientId { get; set; }

        public EnumRelations EnumRelation { get; set; }
    }
}
