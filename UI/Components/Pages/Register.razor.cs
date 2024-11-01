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
        [Inject] IRepository<AccountCheckRegisterRequestDto, AccountCheckRegisterResponseDto> _repoCheckRegister { get; set; } = null!;
        [Inject] IRepository<RegisterAccountRequestDto, ResponseDtoBase> _repoRegister { get; set; } = null!;
        [Inject] IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] IRepository<UploadPhotoToTempRequestDto, UploadPhotoToTempResponseDto> _repoUploadPhotoToTemp { get; set; } = null!;

        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] IDialogService DialogService { get; set; } = null!;
        [Inject] IConfiguration _config { get; set; } = null!;

        RegisterAccountRequestDto registerRequestDto = new RegisterAccountRequestDto
        {
            Name = "Олег и Марина Мск2",
            Email = "olegmar@mail.ru",
            Password = "pass1234",
            Password2 = "pass1234"
        };
        
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool processingPhoto = false;
        bool processingAccount = false;

        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value == true);

        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, bool>
                        {
                            { nameof(registerRequestDto.Name), false },
                            { nameof(registerRequestDto.Email), false },
                            { nameof(registerRequestDto.Password), false },
                            { nameof(registerRequestDto.Password2), false },
                            { nameof(registerRequestDto.Country), false },
                            { nameof(registerRequestDto.Country.Region), false }
                        }
                    }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { nameof(registerRequestDto.Users), false } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { nameof(registerRequestDto.Avatar), false } } } }
            };

            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries.AddRange(apiResponse.Response.Countries);

            // TODO УДАЛИТЬ ЗНАЧЕНИЯ ПО УМОЛЧАНИЮ (OK)
            registerRequestDto.Users = new List<UsersDto> 
            {
                new UsersDto { Id = 0, Name = "Олег", Gender = 0, Weight=80, Height=180, BirthDate = DateTime.Parse("29.01.1977") },
                new UsersDto { Id = 1, Name = "Марина", Gender = 1, Weight=74, Height=173, BirthDate = DateTime.Parse("01.07.1969") }
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
                        registerRequestDto.Country.Id = country.Id;
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
                        registerRequestDto.Country.Region.Id = region.Id;
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

            CheckPanel1Properties(errorMessage, nameof(registerRequestDto.Name), ref NameIconColor);

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

            CheckPanel1Properties(errorMessage, nameof(registerRequestDto.Email), ref EmailIconColor);

            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль должен содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Properties(errorMessage, nameof(registerRequestDto.Password), ref PasswordIconColor);
            return errorMessage;
        }

        Color Password2IconColor = Color.Default;
        string? Password2Validator(string password2)
        {
            string? errorMessage = null;
            if (registerRequestDto.Password != password2) 
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Properties(errorMessage, nameof(registerRequestDto.Password2), ref Password2IconColor);
            return errorMessage;
        }

        Color CountryIconColor = Color.Default;
        string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText)) 
                errorMessage = $"Выберите страну";

            // Сбросим в false регион
            TabPanels[1].Items[nameof(registerRequestDto.Country.Region)] = false;

            CheckPanel1Properties(errorMessage, nameof(registerRequestDto.Country), ref CountryIconColor);
            return errorMessage;
        }

        Color RegionIconColor = Color.Default;
        string? RegionValidator(string region)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(regionText))
                errorMessage = $"Выберите регион";

            CheckPanel1Properties(errorMessage, nameof(registerRequestDto.Country.Region), ref RegionIconColor);
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
            
            if (result != null && result.Canceled == false && registerRequestDto.Users.Contains(user))
                registerRequestDto.Users.Remove(user);

            CheckPanel2Properties();
        }

        async Task AddUserAsync(MouseEventArgs args)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, null } };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Добавление партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
                registerRequestDto.Users.Add((UsersDto)result.Data);

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
                var position = registerRequestDto.Users.IndexOf(user);
                registerRequestDto.Users.RemoveAt(position);
                registerRequestDto.Users.Insert(position, result.Data.DeepCopy<UsersDto>()!);
            }
        }

        void CheckPanel2Properties() =>
            TabPanels[2].Items[nameof(registerRequestDto.Users)] = registerRequestDto.Users.Count == 0 ? false : true;
        #endregion


        #region /// ШАГ 3: ФОТО ///
        async void UploadPhotos(IReadOnlyList<IBrowserFile> browserPhotos)
        {
            if (browserPhotos.Count > 0)
            {
                processingPhoto = true;
                StateHasChanged();

                if (registerRequestDto.Photos == null)
                    registerRequestDto.Photos = new List<PhotosForAccountsDto>();

                foreach (var photo in browserPhotos)
                {
                    var newPhoto = await photo.Upload<PhotosForAccountsDto>(CurrentState.Account?.Token, _repoUploadPhotoToTemp, accountId: 0);

                    if (newPhoto != null)
                    {
                        // Если это первая фотка, то отметим её как аватар
                        if (registerRequestDto.Photos.Count(x => x.IsDeleted == false) == 0)
                            newPhoto.IsAvatar = true;
                        registerRequestDto.Photos.Insert(0, newPhoto);
                    }

                    StateHasChanged();
                    if (registerRequestDto.Photos.Count(x => x.IsDeleted == false) >= 20) break;
                }

                processingPhoto = false;
                StateHasChanged();
            }
        }

        void UpdateCommentPhoto(PhotosForAccountsDto photo, string comment) =>
            photo.Comment = comment;

        void SetAsAvatarPhoto(PhotosForAccountsDto photo)
        {
            registerRequestDto.Photos?.ForEach(x => x.IsAvatar = false);
            photo.IsAvatar = true;
        }
        #endregion


        async void SubmitAsync()
        {
            registerRequestDto.ErrorMessage = null;
            processingAccount = true;

            var response = await _repoRegister.HttpPostAsync(registerRequestDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                registerRequestDto.ErrorMessage = response.Response.ErrorMessage;
                processingAccount = false;
                StateHasChanged();
            }
            else
            {
                LoginRequestDto loginRequestDto = new LoginRequestDto
                {
                    Email = registerRequestDto.Email,
                    Password = registerRequestDto.Password,
                    Remember = registerRequestDto.Remember
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
                    registerRequestDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                    StateHasChanged();
                }
            }
        }

        public void Dispose()
        {
            if (registerRequestDto?.Photos != null)
                foreach (var photo in registerRequestDto.Photos.Where(w => w.Id == 0))
                    if (Directory.Exists(StaticData.TempPhotosDir + "/" + photo.Guid))
                        Directory.Delete(StaticData.TempPhotosDir + "/" + photo.Guid, true);
        }
    }
}
