namespace Common.Dto.Responses
{
    public class GetAdminsForEventsResponseDto : ResponseDtoBase
    {
        public List<AccountsDto>? Admins { get; set; }
    }
}
