namespace Common.Dto.Requests
{
    public class GetNotificationsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Notifications/Get";

        public int Take { get; set; } = 20;

        public bool MarkAsRead { get; set; } = false;
    }
}
