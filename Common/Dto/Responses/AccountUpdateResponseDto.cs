namespace Common.Dto.Responses
{
    public class AccountUpdateResponseDto : ResponseDtoBase
    {
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
    }
}
