using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Extensions;
using Common.JSProcessor;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Net;
using System.Text.RegularExpressions;
using UI.Components.Dialogs;
using UI.Models;

namespace UI.Components.Pages
{
    public partial class Register : IDisposable
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<GetHobbiesRequestDto, GetHobbiesResponseDto> _repoGetHobbies { get; set; } = null!;
        [Inject] IRepository<AccountCheckRegisterRequestDto, AccountCheckRegisterResponseDto> _repoCheckRegister { get; set; } = null!;
        [Inject] IRepository<RegisterAccountRequestDto, ResponseDtoBase> _repoRegister { get; set; } = null!;
        [Inject] IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] IRepository<UploadPhotoToTempRequestDto, UploadPhotoToTempResponseDto> _repoUploadPhotoToTemp { get; set; } = null!;

        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] IDialogService DialogService { get; set; } = null!;
        [Inject] IConfiguration _config { get; set; } = null!;

        RegisterAccountRequestDto accountRequestDto = null!;

        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();
        List<HobbiesDto> hobbies { get; set; } = null!;

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool processingPhoto = false;
        bool processingAccount = false;

        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value == true);
        bool IsPanel4Valid => TabPanels[4].Items.All(x => x.Value == true);

        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, bool>
                        {
                            { nameof(accountRequestDto.Name), false },
                            { nameof(accountRequestDto.Email), false },
                            { nameof(accountRequestDto.Password), false },
                            { nameof(accountRequestDto.Password2), false },
                            { nameof(accountRequestDto.Country), false },
                            { nameof(accountRequestDto.Country.Region), false }
                        }
                    }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { nameof(accountRequestDto.Users), false } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { nameof(accountRequestDto.Hobbies), false } } } },
                { 4, new TabPanel { Items = new Dictionary<string, bool> { { "Photos", false } } } }
            };

            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries.AddRange(apiCountriesResponse.Response.Countries);

            var apiHobbiesResponse = await _repoGetHobbies.HttpPostAsync(new GetHobbiesRequestDto());
            hobbies = apiHobbiesResponse.Response.Hobbies;

            // TODO УДАЛИТЬ ЗНАЧЕНИЯ ПО УМОЛЧАНИЮ (OK)
            accountRequestDto = new RegisterAccountRequestDto
            {
                Name = "Олег и Марина Мск2",
                Email = "olegmar@mail.ru",
                Password = "pass1234",
                Password2 = "pass1234",
                Users = new List<UsersDto>
                {
                    new UsersDto { Id = 0, Name = "Олег", Gender = 0, Weight=80, Height=180, BirthDate = DateTime.Parse("29.01.1977") },
                    new UsersDto { Id = 1, Name = "Марина", Gender = 1, Weight=74, Height=173, BirthDate = DateTime.Parse("01.07.1969") }
                }
            };
        }


        #region /// ШАГ 1: ОБЩЕЕ ///
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
                        accountRequestDto.Country.Id = country.Id;
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
                        accountRequestDto.Country.Region.Id = region.Id;
                }
                _regionText = value;
            }
        }

        Color NameIconColor = Color.Default;
        async Task<string?> NameValidator(string name)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_ACCOUNTS_NAME_MIN)
            {
                errorMessage = $"Имя должно содержать {StaticData.DB_ACCOUNTS_NAME_MIN}-{StaticData.DB_ACCOUNTS_NAME_MAX} символов";
            }
            else
            {
                var apiResponse = await _repoCheckRegister.HttpPostAsync(new AccountCheckRegisterRequestDto { AccountName = name });
                if (apiResponse.Response.AccountNameExists)
                    errorMessage = $"Это имя уже занято. Выберите другое.";
            }

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Name), ref NameIconColor);

            return errorMessage;
        }

        Color EmailIconColor = Color.Default;
        async Task<string?> EmailValidator(string email)
        {
            string? errorMessage = null;

            if (string.IsNullOrWhiteSpace(email) || email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN)
            {
                errorMessage = $"Email может содержать {StaticData.DB_ACCOUNTS_EMAIL_MIN}-{StaticData.DB_ACCOUNTS_EMAIL_MAX} символов";
            }
            else if (!Regex.IsMatch(email, @"^[a-z0-9_\.-]{1,32}@[a-z0-9\.-]{1,32}\.[a-z]{2,8}$"))
            {
                errorMessage = $"Проверьте корректность email";
            }
            else
            {
                var apiResponse = await _repoCheckRegister.HttpPostAsync(new AccountCheckRegisterRequestDto { AccountEmail = email });
                if (apiResponse.Response.AccountEmailExists)
                    errorMessage = $"Этот email уже зарегистрирован. Забыли пароль?";
            }

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Email), ref EmailIconColor);

            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль должен содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Password), ref PasswordIconColor);
            return errorMessage;
        }

        Color Password2IconColor = Color.Default;
        string? Password2Validator(string password2)
        {
            string? errorMessage = null;
            if (accountRequestDto.Password != password2) 
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Password2), ref Password2IconColor);
            return errorMessage;
        }

        Color CountryIconColor = Color.Default;
        string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText)) 
                errorMessage = $"Выберите страну";

            // Сбросим в false регион
            TabPanels[1].Items[nameof(accountRequestDto.Country.Region)] = false;

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Country), ref CountryIconColor);
            return errorMessage;
        }

        Color RegionIconColor = Color.Default;
        string? RegionValidator(string region)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(regionText))
                errorMessage = $"Выберите регион";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Country.Region), ref RegionIconColor);
            return errorMessage;
        }

        void CheckPanel1Properties(string? errorMessage, string property, ref Color iconColor)
        {
            if (errorMessage == null)
            {
                TabPanels[1].Items[property] = true;
                iconColor = Color.Success;
            }
            else
            {
                TabPanels[1].Items[property] = false;
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
            
            if (result != null && result.Canceled == false && accountRequestDto.Users.Contains(user))
                accountRequestDto.Users.Remove(user);

            CheckPanel2Properties();
        }

        async Task AddUserAsync(MouseEventArgs args)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, null } };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Добавление партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
                accountRequestDto.Users.Add((UsersDto)result.Data);

            CheckPanel2Properties();
        }

        async Task UpdateUserAsync(UsersDto user)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, user } };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Редактирование партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
            {
                var position = accountRequestDto.Users.IndexOf(user);
                accountRequestDto.Users.RemoveAt(position);
                accountRequestDto.Users.Insert(position, result.Data.DeepCopy<UsersDto>()!);
            }
        }

        void CheckPanel2Properties() =>
            TabPanels[2].Items[nameof(accountRequestDto.Users)] = accountRequestDto.Users.Count == 0 ? false : true;
        #endregion


        #region /// ШАГ 3: ХОББИ ///
        void OnHobbyChanged(HobbiesDto hobby)
        {
            if (accountRequestDto.Hobbies != null)
            {
                var index = accountRequestDto.Hobbies.FindIndex(x => x.Id == hobby.Id);
                if (index >= 0)
                    accountRequestDto.Hobbies.RemoveAt(index);
                else
                    accountRequestDto.Hobbies.Add(hobby);
            }
            else
                accountRequestDto.Hobbies = [hobby];

            CheckPanel3Properties();
        }

        void CheckPanel3Properties()
        {
            if (accountRequestDto.Hobbies != null && accountRequestDto.Hobbies.Any())
                TabPanels[3].Items[nameof(accountRequestDto.Hobbies)] = true;
            else
                TabPanels[3].Items[nameof(accountRequestDto.Hobbies)] = false;
            StateHasChanged();
        }
        #endregion


        #region /// ШАГ 4: ФОТО ///
        async void UploadPhotos(IReadOnlyList<IBrowserFile> browserPhotos)
        {
            if (browserPhotos.Count > 0)
            {
                processingPhoto = true;
                StateHasChanged();

                if (accountRequestDto.Photos == null)
                    accountRequestDto.Photos = new List<PhotosForAccountsDto>();

                foreach (var photo in browserPhotos)
                {
                    var newPhoto = await photo.Upload<PhotosForAccountsDto>(CurrentState.Account?.Token, _repoUploadPhotoToTemp, accountId: 0);

                    if (newPhoto != null)
                    {
                        // Если это первая фотка, то отметим её как аватар
                        if (accountRequestDto.Photos.Count(x => x.IsDeleted == false) == 0)
                            newPhoto.IsAvatar = true;
                        accountRequestDto.Photos.Insert(0, newPhoto);
                    }

                    StateHasChanged();
                    if (accountRequestDto.Photos.Count(x => x.IsDeleted == false) >= 20) break;
                }

                processingPhoto = false;
                TabPanels[4].Items["Photos"] = true;
                StateHasChanged();
            }
        }

        void UpdateCommentPhoto(PhotosForAccountsDto photo, string comment) =>
            photo.Comment = comment;

        void SetAsAvatarPhoto(PhotosForAccountsDto photo)
        {
            accountRequestDto.Photos?.ForEach(x => x.IsAvatar = false);
            photo.IsAvatar = true;
        }
        #endregion


        async void SubmitAsync()
        {
            accountRequestDto.ErrorMessage = null;
            processingAccount = true;

            var response = await _repoRegister.HttpPostAsync(accountRequestDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                accountRequestDto.ErrorMessage = response.Response.ErrorMessage;
                processingAccount = false;
                StateHasChanged();
            }
            else
            {
                LoginRequestDto loginRequestDto = new LoginRequestDto
                {
                    Email = accountRequestDto.Email,
                    Password = accountRequestDto.Password,
                    Remember = accountRequestDto.Remember
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

                    await _JSProcessor.Redirect("/");
                }
                else
                {
                    accountRequestDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                    StateHasChanged();
                }
            }
        }

        public void Dispose()
        {
            if (accountRequestDto?.Photos != null)
                foreach (var photo in accountRequestDto.Photos.Where(w => w.Id == 0))
                    if (Directory.Exists(StaticData.TempPhotosDir + "/" + photo.Guid))
                        Directory.Delete(StaticData.TempPhotosDir + "/" + photo.Guid, true);
        }
    }
}
