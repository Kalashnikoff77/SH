using Common.Dto;

namespace Common.Models
{
    public class AccountUpdateModel : ModelBase
    {
        public Guid Guid { get; set; }

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string NewPassword1 { get; set; } = null!;
        public string NewPassword2 { get; set; } = null!;

        public Informing Informing { get; set; } = null!;

        public bool IsConfirmed { get; set; }


        public List<UsersDto> Users { get; set; } = null!;

        public CountriesDto Country { get; set; } = null!;

        public AccountsPhotosDto? Avatar { get; set; }

        public List<AccountsHobbiesDto>? Hobbies { get; set; }


        public string? Error { get; set; } = null;
    }
}
