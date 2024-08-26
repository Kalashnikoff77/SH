namespace Common.Dto.Requests
{
    public class GetAdminsForEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Accounts/GetAdminsForEvents";

        public List<int>? AdminsIds { get; set; }
    }
}
