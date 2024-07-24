using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetAccountsResponseDto : ResponseDtoBase
    {
        public List<AccountsViewDto> Accounts { get; set; } = null!;
    }
}
