using Common.Enums;

namespace Common.Models
{
    public class GetRelationsForAccountsModel : ModelBase
    {
        public int AccountId { get; set; }
        public EnumRelations Relation { get; set; }
    }
}
