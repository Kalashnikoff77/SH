namespace Common.Models.SignalR
{
    public class SignalGlobalRequest
    {
        public OnScheduleChanged? OnScheduleChanged { get; set; }
    }


    public class OnScheduleChanged
    {
        public int ScheduleId { get; set; }
        public string Message { get; set; } = null!;
    }
}
