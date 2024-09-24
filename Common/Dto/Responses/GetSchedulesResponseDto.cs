﻿using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetSchedulesResponseDto : ResponseDtoBase
    {
        public List<SchedulesForEventsViewDto> Events { get; set; } = new List<SchedulesForEventsViewDto>();
    }
}