﻿namespace Common.Dto.Requests
{
    public class AccountRegisterRequestDto : RequestDtoBase
    {
        public override string Uri => "/Accounts/Register";

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string Password2 { get; set; } = null!;

        public string OriginalPhoto { get; set; } = null!;

        public Informing Informing { get; set; } = new Informing();

        public CountriesDto Country { get; set; } = null!;

        public List<UsersDto> Users { get; set; } = null!;

        public bool IsConfirmed { get; set; } = false;

        public bool AcceptTerms { get; set; } = true;


        public string? Avatar { get; set; }
        public string? ErrorRegisterMessage { get; set; }
        public bool Remember { get; set; }
    }
}
