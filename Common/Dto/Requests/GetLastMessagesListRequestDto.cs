﻿namespace Common.Dto.Requests
{
    public class GetLastMessagesListRequestDto : RequestDtoBase
    {
        public override string Uri => "/Messages/GetLastMessagesList";

        public int Take { get; set; } = 20;
    }
}
