namespace Common.Dto.Views
{
    public class AccountsForSchedulesViewDto : AccountsForSchedulesDto
    {
        public AccountsViewDto Account { get; set; } = null!;
    }
}
