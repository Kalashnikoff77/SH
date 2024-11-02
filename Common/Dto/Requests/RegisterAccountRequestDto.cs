namespace Common.Dto.Requests
{
    public class RegisterAccountRequestDto : RequestDtoBase
    {
        public override string Uri => "/Accounts/Register";

        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string Password2 { get; set; } = null!;

        public Informing Informing { get; set; } = new Informing();

        public List<UsersDto> Users { get; set; } = new List<UsersDto>();

        public CountriesDto Country { get; set; } = new CountriesDto() { Region = new RegionsDto() };

        public List<HobbiesDto> Hobbies { get; set; } = new List<HobbiesDto>();

        public List<PhotosForAccountsDto> Photos { get; set; } = null!;

        public bool IsConfirmed { get; set; } = false;

        public bool AcceptTerms { get; set; } = true;

        public bool Remember { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
