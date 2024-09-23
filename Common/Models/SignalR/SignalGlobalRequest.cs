namespace Common.Models.SignalR
{
    public class SignalGlobalRequest
    {
        public OnEventDiscussionAdded? OnEventDiscussionAdded { get; set; }
    }


    public class OnEventDiscussionAdded
    {
        public int ScheduleId { get; set; }
        public string Message { get; set; } = null!;
    }
}
