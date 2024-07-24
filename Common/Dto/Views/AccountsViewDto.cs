namespace Common.Dto.Views
{
    public class AccountsViewDto : AccountsDto
    {
        public List<UsersDto>? Users { get; set; }

        public CountriesDto? Country { get; set; }

        public AccountsPhotosDto? Avatar { get; set; }

        public List<AccountsPhotosDto>? Photos { get; set; }

        public List<EventsViewDto>? Events { get; set; }

        public List<AccountsHobbiesDto>? Hobbies { get; set; }

        public List<AccountsRelationsDto>? Relations { get; set; }
    }
}
