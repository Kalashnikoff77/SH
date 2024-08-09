using Common.Dto.Views;
using Common.Dto;
using Common.Models;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.JSProcessor;
using Common.Repository;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using Common;
using MudBlazor;

namespace UI.Components.Pages
{
    public partial class Register
    {
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<AccountRegisterModel, AccountRegisterRequestDto, ResponseDtoBase> _repoRegister { get; set; } = null!;
        [Inject] IRepository<LoginModel, LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        AccountRegisterModel registerModel = new AccountRegisterModel();
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }

        #region /// ШАГ 1: ОБЩЕЕ ///
        Color Panel1Color = Color.Default;
        string? _countryText;
        string? countryText { 
            get => _countryText;
            set
            {
                if (value != null)
                {
                    var country = countries.Where(c => c.Name == value)?.First();
                    if (country != null)
                    {
                        registerModel.Country.Id = country.Id;
                        regions = countries
                            .Where(x => x.Id == country.Id).FirstOrDefault()?
                            .Regions?.Select(s => s).ToList();
                    }
                }
                _countryText = value;
                _regionText = null;
            }
        }

        string? _regionText;
        string? regionText
        {
            get => _regionText;
            set
            {
                if (value != null && regions != null)
                {
                    var region = regions.Where(c => c.Name == value)?.First();
                    if (region != null)
                        registerModel.Country.Region.Id = region.Id;
                }
                _regionText = value;
            }
        }

        async Task<IEnumerable<string>?> SearchCountry(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
                return countries.Select(s => s.Name);

            return countries?.Select(s => s.Name)
                .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        async Task<IEnumerable<string>?> SearchRegion(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
                return regions?.Select(s => s.Name);
            
            return regions?.Select(s => s.Name)
                .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        Color NameIconColor = Color.Default;
        string? NameValidator(string name)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_ACCOUNTS_NAME_MIN)
                errorMessage = $"Имя должно содержать {StaticData.DB_ACCOUNTS_NAME_MIN}-{StaticData.DB_ACCOUNTS_NAME_MAX} символов";

            CheckPanel1Validation(ref NameIconColor, errorMessage);
            return errorMessage;
        }

        Color EmailIconColor = Color.Default;
        string? EmailValidator(string email)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(email) || email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN)
                errorMessage = $"Email может содержать {StaticData.DB_ACCOUNTS_EMAIL_MIN}-{StaticData.DB_ACCOUNTS_EMAIL_MAX} символов";

            CheckPanel1Validation(ref EmailIconColor, errorMessage);
            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль может содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Validation(ref PasswordIconColor, errorMessage);
            return errorMessage;
        }

        Color Password2IconColor = Color.Default;
        string? Password2Validator(string password)
        {
            string? errorMessage = null;
            if (registerModel.Password != password)
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Validation(ref Password2IconColor, errorMessage);
            return errorMessage;
        }

        void CheckPanel1Validation(ref Color color, string? errorMessage)
        {
            color = errorMessage == null ? Color.Success : Color.Error;

            if (NameIconColor == Color.Error || EmailIconColor == Color.Error || PasswordIconColor == Color.Error || Password2IconColor == Color.Error)
            {
                Panel2Disabled = true;
                Panel2Expanded = false;
                Panel1Color = Color.Error;
            }

            if (NameIconColor == Color.Success && EmailIconColor == Color.Success && PasswordIconColor == Color.Success && Password2IconColor == Color.Success)
            {
                Panel2Disabled = false;
                Panel2Expanded = true;
                Panel1Color = Color.Success;
            }
            StateHasChanged();
        }
        #endregion


        #region /// ШАГ 2: АВАТАР ///
        bool Panel2Disabled = true;
        bool Panel2Expanded = false;
        #endregion

        #region /// ШАГ 3: ПАРТНЁРЫ ///
        bool Panel3Disabled = true;
        #endregion
    }
}
