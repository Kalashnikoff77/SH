using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Extensions;
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
    public partial class Profile : IDisposable
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<GetHobbiesRequestDto, GetHobbiesResponseDto> _repoGetHobbies { get; set; } = null!;
        [Inject] IRepository<AccountCheckUpdateRequestDto, AccountCheckUpdateResponseDto> _repoCheckUpdate { get; set; } = null!;
        [Inject] IRepository<UpdateAccountRequestDto, ResponseDtoBase> _repoUpdate { get; set; } = null!;
        [Inject] IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] IRepository<UploadPhotoToTempRequestDto, UploadPhotoToTempResponseDto> _repoUploadPhotoToTemp { get; set; } = null!;

        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IDialogService DialogService { get; set; } = null!;
        [Inject] IConfiguration _config { get; set; } = null!;
        [Inject] IMapper _mapper { get; set; } = null!;

        UpdateAccountRequestDto accountRequestDto = null!;

        List<CountriesViewDto> countries { get; set; } = null!;
        List<RegionsDto> regions { get; set; } = null!;
        List<HobbiesDto> hobbies { get; set; } = null!;

        bool processingAccount, processingPhoto, isDataSaved = false;
        /// <summary>
        /// Для предотвращения повторного выполнения OnParametersSet (выполняется при переходе на другую ссылку)
        /// </summary>
        bool isFirstSetParameters = true;

        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value == true);
        bool IsPanel4Valid => TabPanels[4].Items.All(x => x.Value == true);

        static IReadOnlyDictionary<short, TabPanel> TabPanels = new Dictionary<short, TabPanel>
        {
            { 1, new TabPanel { Items = new Dictionary<string, bool>
                    {
                        { nameof(accountRequestDto.Name), true },
                        { nameof(accountRequestDto.Email), true },
                        { nameof(accountRequestDto.Password), true },
                        { nameof(accountRequestDto.Password2), true },
                        { nameof(accountRequestDto.Country), true },
                        { nameof(accountRequestDto.Country.Region), true }
                    }
                }
            },
            { 2, new TabPanel { Items = new Dictionary<string, bool> { { nameof(accountRequestDto.Users), true } } } },
            { 3, new TabPanel { Items = new Dictionary<string, bool> { { nameof(accountRequestDto.Hobbies), true } } } },
            { 4, new TabPanel { Items = new Dictionary<string, bool> { { "Photos", true } } } }
        };


        protected override async Task OnInitializedAsync()
        {
            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries = apiCountriesResponse.Response.Countries;

            var apiHobbiesResponse = await _repoGetHobbies.HttpPostAsync(new GetHobbiesRequestDto());
            hobbies = apiHobbiesResponse.Response.Hobbies;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null && isFirstSetParameters)
            {
                accountRequestDto = _mapper.Map<UpdateAccountRequestDto>(CurrentState.Account);

                countryText = CurrentState.Account.Country!.Name;
                regionText = CurrentState.Account.Country!.Region.Name;

                var storage = await _protectedLocalStore.GetAsync<LoginRequestDto>(nameof(LoginRequestDto));
                if (storage.Success)
                    accountRequestDto.Remember = true;

                isFirstSetParameters = false;
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
                        accountRequestDto.Country.Id = country.Id;
                        regions = countries
                            .Where(x => x.Id == country.Id).First()
                            .Regions!.Select(s => s).ToList();
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
                errorMessage = $"Имя должно содержать {StaticData.DB_ACCOUNTS_NAME_MIN}-{StaticData.DB_ACCOUNTS_NAME_MAX} символов";

            var apiResponse = await _repoCheckUpdate.HttpPostAsync(new AccountCheckUpdateRequestDto { AccountName = name, Token = CurrentState.Account!.Token });
            if (apiResponse.Response.AccountNameExists)
                errorMessage = $"Это имя уже занято. Выберите другое.";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Name), ref NameIconColor);

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
            {
                var index = accountRequestDto.Users.IndexOf(user);
                if (index >= 0)
                    accountRequestDto.Users[index].IsDeleted = true;
            }
            CheckPanel2Properties();
        }

        async Task UpdateUserDialogAsync(UsersDto user)
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

        void CheckPanel2Properties() =>
            TabPanels[2].Items[nameof(accountRequestDto.Users)] = accountRequestDto.Users.Where(w => !w.IsDeleted).Count() == 0 ? false : true;
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
                    var newPhoto = await photo.Upload<PhotosForAccountsDto>(CurrentState.Account?.Token, _repoUploadPhotoToTemp, accountId: CurrentState.Account!.Id);

                    if (newPhoto != null)
                        accountRequestDto.Photos.Insert(0, newPhoto);

                    StateHasChanged();
                    if (accountRequestDto.Photos.Count(x => x.IsDeleted == false) >= 20) break;
                }

                processingPhoto = false;
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
            StateHasChanged();

            var response = await _repoUpdate.HttpPostAsync(accountRequestDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                accountRequestDto.ErrorMessage = response.Response.ErrorMessage;
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

                    isDataSaved = true;
                }
                else
                {
                    accountRequestDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                }
            }

            processingAccount = false;
            CurrentState.StateHasChanged();
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
