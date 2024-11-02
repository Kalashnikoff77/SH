namespace Common.Dto.Requests
{
    public abstract class AccountRequestDtoBase : RequestDtoBase
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string Password2 { get; set; } = null!;

        public Informing Informing { get; set; } = null!;

        public CountriesDto Country { get; set; } = null!;

        public List<UsersDto> Users { get; set; } = null!;

        public List<HobbiesDto>? Hobbies { get; set; }

        public List<PhotosForAccountsDto> Photos { get; set; } = null!;

        public bool Remember { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
