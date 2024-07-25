﻿namespace Common.Models
{
    public class GetAccountModel : ModelBase
    {
        public int? Id { get; set; }
        public Guid? Guid { get; set; }

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