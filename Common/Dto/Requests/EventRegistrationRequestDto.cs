﻿namespace Common.Dto.Requests
{
    public class EventRegistrationRequestDto : RequestDtoBase
    {
        public override string Uri => "/Accounts/EventRegistration";

        public int ScheduleId { get; set; }
    }
}