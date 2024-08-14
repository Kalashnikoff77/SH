﻿namespace Common.Models
{
    public class GetEventDiscussionsModel : ModelBase
    {
        public int EventId { get; set; }

        // Удалить, т.к. есть в ModelBase
        //public int Take { get; set; } = StaticData.EVENT_DISCUSSIONS_PER_BLOCK;

        public int? GetPreviousFromId { get; set; }
        public int? GetNextAfterId { get; set; }
    }
}
