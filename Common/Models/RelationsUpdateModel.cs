using Common.Enums;

namespace Common.Models
{
    public class RelationsUpdateModel : ModelBase
    {
        public int RecipientId { get; set; }
        public EnumRelations EnumRelation { get; set; }
    }
}
