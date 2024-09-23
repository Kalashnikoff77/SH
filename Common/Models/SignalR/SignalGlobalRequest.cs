﻿namespace Common.Models.SignalR
{
    public class SignalGlobalRequest
    {
        public OnEventDiscussionAdded? OnEventDiscussionAdded { get; set; }
    }


    public class OnEventDiscussionAdded
    {
        public int EventId { get; set; }
    }
}
