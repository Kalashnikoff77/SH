using Common.Dto;

namespace Common.Models
{
    public class AccountRegisterModel : ModelBase //AccountBaseModel
    {
        // TODO REMOVE
        public AccountRegisterModel()
        {
            Name = "Олег и Марина Мск";
            Email = "oleg_reg@mail.ru";
            Password = "pass2";
            Password2 = "pass2";
            Country = new CountriesDto
            {
                Region = new RegionsDto
                {
                    Id = 1
                }
            };
        }

        public Guid Guid { get; set; }

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string Password2 { get; set; } = null!;

        public string? PreviewPhoto { get; set; }
        public string? OriginalPhoto { get; set; }

        public bool IsConfirmed { get; set; }

        public bool Remember { get; set; }


        public List<UsersDto> Users { get; set; } = new List<UsersDto>();

        public CountriesDto Country { get; set; } = new CountriesDto() { Region = new RegionsDto() };

        public AccountsPhotosDto? Avatar { get; set; }

        public List<AccountsHobbiesDto>? Hobbies { get; set; }


        public string? Error { get; set; } = null;
        public string? ErrorPhoto { get; set; } = null;

        public bool AcceptTerms { get; set; } = true;
    }
}
