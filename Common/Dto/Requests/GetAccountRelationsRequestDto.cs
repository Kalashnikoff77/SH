﻿using Common.Enums;

namespace Common.Dto.Requests
{
    public class GetAccountRelationsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Accounts/GetRelations";

        public int AccountId { get; set; }
        public EnumRelations Relation { get; set; }
    }
}