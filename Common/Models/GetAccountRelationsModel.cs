using Common.Enums;

namespace Common.Models
{
    public class GetAccountRelationsModel : ModelBase
    {
        public int AccountId { get; set; }
        public EnumRelations Relation { get; set; }
    }
}
