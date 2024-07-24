using Common.Dto.Views;

namespace Common.Dto.Responses
{
    public class GetAccountPhotosResponseDto : ResponseDtoBase
    {
        public List<AccountsPhotosViewDto> Photos { get; set; } = null!;
    }
}
