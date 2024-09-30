using AutoMapper;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Dto;
using Common.JSProcessor;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using UI.Models;
using Common.Extensions;
using Common.Models;
using Microsoft.AspNetCore.Components.Web;
using System.Net;
using System.Text.RegularExpressions;
using UI.Components.Shared.Dialogs;

namespace UI.Components.Pages.Profile
{
    public partial class Tab_Settings
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public EventCallback UpdateProfileCallback { get; set; }

        [Inject] IRepository<GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<GetHobbiesRequestDto, GetHobbiesResponseDto> _repoGetHobbies { get; set; } = null!;
        [Inject] IRepository<AccountCheckUpdateRequestDto, AccountCheckUpdateResponseDto> _repoCheckUpdate { get; set; } = null!;
        [Inject] IRepository<AccountUpdateRequestDto, ResponseDtoBase> _repoUpdate { get; set; } = null!;
        [Inject] IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;

        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] IDialogService DialogService { get; set; } = null!;
        [Inject] IConfiguration _config { get; set; } = null!;

        [Inject] IMapper _mapper { get; set; } = null!;

        AccountUpdateRequestDto accountUpdateDto = null!;
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();
        List<HobbiesDto>? hobbies { get; set; }

        bool processingAccount = false;
        bool isDataSaved = false;

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value.IsValid == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value.IsValid == true);

        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel {
                    Items = new Dictionary<string, TabPanelItem>
                        {
                            { nameof(accountUpdateDto.Name), new TabPanelItem() { IsValid = true } },
                            { nameof(accountUpdateDto.Email), new TabPanelItem() { IsValid = true } },
                            { nameof(accountUpdateDto.Password), new TabPanelItem() { IsValid = true } },
                            { nameof(accountUpdateDto.Password2), new TabPanelItem() { IsValid = true } },
                            { nameof(accountUpdateDto.Country), new TabPanelItem() { IsValid = true } },
                            { nameof(accountUpdateDto.Country.Region), new TabPanelItem() { IsValid = true } }
                        }
                    }
                },
                { 2, new TabPanel {
                    Items = new Dictionary<string, TabPanelItem>
                        {
                            { nameof(accountUpdateDto.Users), new TabPanelItem() { IsValid = true } }
                        }
                    }
                }
            };

            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries.AddRange(apiCountriesResponse.Response.Countries);

            var apiHobbiesResponse = await _repoGetHobbies.HttpPostAsync(new GetHobbiesRequestDto());
            hobbies = apiHobbiesResponse.Response.Hobbies;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null)
            {
                accountUpdateDto = _mapper.Map<AccountUpdateRequestDto>(CurrentState.Account);

                countryText = CurrentState.Account.Country!.Name;
                regionText = CurrentState.Account.Country!.Region.Name;

                var storage = await _protectedLocalStore.GetAsync<LoginRequestDto>(nameof(LoginRequestDto));
                if (storage.Success)
                    accountUpdateDto.Remember = true;
            }
        }


        #region /// ШАГ 1: ОБЩЕЕ ///
        string? _countryText;
        string? countryText
        {
            get => _countryText;
            set
            {
                if (value != null && countries.Any())
                {
                    var country = countries.Where(c => c.Name == value)?.First();
                    if (country != null)
                    {
                        accountUpdateDto.Country.Id = country.Id;
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
                if (value != null && countries.Any() && regions != null)
                {
                    var region = regions.Where(c => c.Name == value)?.First();
                    if (region != null)
                        accountUpdateDto.Country.Region.Id = region.Id;
                }
                _regionText = value;
            }
        }

        Color NameIconColor = Color.Default;
        async Task<string?> NameValidator(string name)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_ACCOUNTS_NAME_MIN)
                errorMessage = $"Имя должно содержать {StaticData.DB_ACCOUNTS_NAME_MIN}-{StaticData.DB_ACCOUNTS_NAME_MAX} символов";

            var apiResponse = await _repoCheckUpdate.HttpPostAsync(new AccountCheckUpdateRequestDto { AccountName = name, Token = CurrentState.Account!.Token });
            if (apiResponse.Response.AccountNameExists)
                errorMessage = $"Это имя уже занято. Выберите другое.";

            CheckPanel1Properties(errorMessage, nameof(accountUpdateDto.Name), ref NameIconColor);

            return errorMessage;
        }

        Color EmailIconColor = Color.Default;
        async Task<string?> EmailValidator(string email)
        {
            string? errorMessage = null;

            if (string.IsNullOrWhiteSpace(email) || email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN)
                errorMessage = $"Email может содержать {StaticData.DB_ACCOUNTS_EMAIL_MIN}-{StaticData.DB_ACCOUNTS_EMAIL_MAX} символов";

            if (!Regex.IsMatch(email, @"^[a-z0-9_\.-]{1,32}@[a-z0-9\.-]{1,32}\.[a-z]{2,8}$"))
                errorMessage = $"Проверьте корректность email";

            var apiResponse = await _repoCheckUpdate.HttpPostAsync(new AccountCheckUpdateRequestDto { AccountEmail = email, Token = CurrentState.Account!.Token });
            if (apiResponse.Response.AccountEmailExists)
                errorMessage = $"Этот email уже занят другим пользователем.";

            CheckPanel1Properties(errorMessage, nameof(accountUpdateDto.Email), ref EmailIconColor);

            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль должен содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Properties(errorMessage, nameof(accountUpdateDto.Password), ref PasswordIconColor);
            return errorMessage;
        }

        Color Password2IconColor = Color.Default;
        string? Password2Validator(string password2)
        {
            string? errorMessage = null;
            if (accountUpdateDto.Password != password2)
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Properties(errorMessage, nameof(accountUpdateDto.Password2), ref Password2IconColor);
            return errorMessage;
        }

        Color CountryIconColor = Color.Default;
        string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText))
                errorMessage = $"Выберите страну";

            CheckPanel1Properties(errorMessage, nameof(accountUpdateDto.Country), ref CountryIconColor);
            return errorMessage;
        }

        Color RegionIconColor = Color.Default;
        string? RegionValidator(string region)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(regionText))
                errorMessage = $"Выберите регион";

            CheckPanel1Properties(errorMessage, nameof(accountUpdateDto.Country.Region), ref RegionIconColor);
            return errorMessage;
        }

        void CheckPanel1Properties(string? errorMessage, string property, ref Color iconColor)
        {
            if (errorMessage == null)
            {
                TabPanels[1].Items[property].IsValid = true;
                iconColor = Color.Success;
            }
            else
            {
                TabPanels[1].Items[property].IsValid = false;
                iconColor = Color.Error;
            }
            StateHasChanged();
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
        #endregion


        #region /// ШАГ 2: ПАРТНЁРЫ ///
        async Task DeleteUserDialogAsync(UsersDto user)
        {
            var parameters = new DialogParameters<ConfirmDialog>
            {
                { x => x.ContentText, $"Удалить партнёра {user.Name}?" },
                { x => x.ButtonText, "Удалить" },
                { x => x.Color, Color.Error }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var resultDialog = await DialogService.ShowAsync<ConfirmDialog>($"Удаление {user.Name}", parameters, options);
            var result = await resultDialog.Result;

            if (result != null && result.Canceled == false && accountUpdateDto.Users.Contains(user))
            {
                var index = accountUpdateDto.Users.IndexOf(user);
                if (index >= 0)
                    accountUpdateDto.Users[index].IsDeleted = true;
            }
            CheckPanel2Properties();
        }

        async Task AddUserAsync(MouseEventArgs args)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, null } };
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Добавление партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
                accountUpdateDto.Users.Add((UsersDto)result.Data);

            CheckPanel2Properties();
        }

        async Task UpdateUserAsync(UsersDto user)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, user } };
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Редактирование партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
            {
                var position = accountUpdateDto.Users.IndexOf(user);
                accountUpdateDto.Users.RemoveAt(position);
                accountUpdateDto.Users.Insert(position, result.Data.DeepCopy<UsersDto>()!);
            }
        }

        void CheckPanel2Properties() =>
            TabPanels[2].Items[nameof(accountUpdateDto.Users)].IsValid = accountUpdateDto.Users.Where(w => !w.IsDeleted).Count() == 0 ? false : true;
        #endregion


        #region /// ШАГ 3: ХОББИ ///
        void OnHobbyChanged(HobbiesDto hobby)
        {
            if (CurrentState.Account != null)
            {
                if (CurrentState.Account.Hobbies != null)
                {
                    var index = CurrentState.Account.Hobbies.FindIndex(x => x.Id == hobby.Id);
                    if (index >= 0)
                        CurrentState.Account.Hobbies.RemoveAt(index);
                    else
                        CurrentState.Account.Hobbies.Add(hobby);
                }
                else
                    CurrentState.Account.Hobbies = [hobby];
                
                accountUpdateDto.Hobbies = CurrentState.Account.Hobbies;
            }
        }
        #endregion

        async void SubmitAsync()
        {
            accountUpdateDto.ErrorMessage = null;
            processingAccount = true;

            var response = await _repoUpdate.HttpPostAsync(accountUpdateDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                accountUpdateDto.ErrorMessage = response.Response.ErrorMessage;
            }
            else
            {
                LoginRequestDto loginRequestDto = new LoginRequestDto
                {
                    Email = accountUpdateDto.Email,
                    Password = accountUpdateDto.Password,
                    Remember = accountUpdateDto.Remember
                };

                var apiResponse = await _repoLogin.HttpPostAsync(loginRequestDto);

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse.Response.Account!.Token = StaticData.GenerateToken(apiResponse.Response.Account.Id, apiResponse.Response.Account.Guid, _config);
                    CurrentState.SetAccount(apiResponse.Response.Account);

                    if (loginRequestDto.Remember)
                        await _protectedLocalStore.SetAsync(nameof(LoginRequestDto), loginRequestDto);
                    else
                        await _protectedSessionStore.SetAsync(nameof(LoginRequestDto), loginRequestDto);

                    isDataSaved = true;
                }
                else
                {
                    accountUpdateDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                }
            }

            processingAccount = false;
            await UpdateProfileCallback.InvokeAsync();
        }
    }
}
