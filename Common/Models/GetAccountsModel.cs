using Common.Enums;

namespace Common.Models
{
    public class GetAccountsModel : ModelBase
    {
        public EnumOrders? Order { get; set; }

        public int? Id { get; set; }
        public Guid? Guid { get; set; }

        public int Take { get; set; } = 10;
        public int Skip { get; set; } = 0;

        public bool IsUsersIncluded { get; set; } = false;
        public bool IsAvatarIncluded { get; set; } = false;
        public bool IsPhotosIncluded { get; set; } = false;
        public bool IsRegionIncluded { get; set; } = false;
        public bool IsVisitsIncluded { get; set; } = false;
        public bool IsRelationsIncluded { get; set; } = false;
        public bool IsHobbiesIncluded { get; set; } = false;
        public bool IsEventsIncluded { get; set; } = false;
    }
}
