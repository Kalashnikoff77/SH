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

        UpdateAccountRequestDto updateRequestDto = null!;
        List<CountriesViewDto> countries { get; set; } = null!;
        List<RegionsDto> regions { get; set; } = null!;
        List<HobbiesDto> hobbies { get; set; } = null!;

        bool processingAccount, processingPhoto, isDataSaved = false;

        /// <summary>
        /// Для предотвращения повторного выполнения OnParametersSet (выполняется при переходе на другую ссылку)
        /// </summary>
        bool isFirstSetParameters = true;

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value == true);
        bool IsPanel4Valid => TabPanels[4].Items.All(x => x.Value == true);

        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel {
                    Items = new Dictionary<string, bool>
                        {
                            { nameof(updateRequestDto.Name), true },
                            { nameof(updateRequestDto.Email), true },
                            { nameof(updateRequestDto.Password), true },
                            { nameof(updateRequestDto.Password2), true },
                            { nameof(updateRequestDto.Country), true },
                            { nameof(updateRequestDto.Country.Region), true }
                        }
                    }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { nameof(updateRequestDto.Users), true } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { nameof(updateRequestDto.Hobbies), true } } } },
                { 4, new TabPanel { Items = new Dictionary<string, bool> { { "Photos", true } } } }
            };

            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries = apiCountriesResponse.Response.Countries;

            var apiHobbiesResponse = await _repoGetHobbies.HttpPostAsync(new GetHobbiesRequestDto());
            hobbies = apiHobbiesResponse.Response.Hobbies;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null && isFirstSetParameters)
            {
                updateRequestDto = _mapper.Map<UpdateAccountRequestDto>(CurrentState.Account);

                countryText = CurrentState.Account.Country!.Name;
                regionText = CurrentState.Account.Country!.Region.Name;

                var storage = await _protectedLocalStore.GetAsync<LoginRequestDto>(nameof(LoginRequestDto));
                if (storage.Success)
                    updateRequestDto.Remember = true;

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
                        updateRequestDto.Country.Id = country.Id;
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
                        updateRequestDto.Country.Region.Id = region.Id;
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

            CheckPanel1Properties(errorMessage, nameof(updateRequestDto.Name), ref NameIconColor);

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

            CheckPanel1Properties(errorMessage, nameof(updateRequestDto.Email), ref EmailIconColor);

            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль должен содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Properties(errorMessage, nameof(updateRequestDto.Password), ref PasswordIconColor);
            return errorMessage;
        }

        Color Password2IconColor = Color.Default;
        string? Password2Validator(string password2)
        {
            string? errorMessage = null;
            if (updateRequestDto.Password != password2)
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Properties(errorMessage, nameof(updateRequestDto.Password2), ref Password2IconColor);
            return errorMessage;
        }

        Color CountryIconColor = Color.Default;
        string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText))
                errorMessage = $"Выберите страну";

            // Сбросим в false регион
            TabPanels[1].Items[nameof(updateRequestDto.Country.Region)] = false;

            CheckPanel1Properties(errorMessage, nameof(updateRequestDto.Country), ref CountryIconColor);
            return errorMessage;
        }

        Color RegionIconColor = Color.Default;
        string? RegionValidator(string region)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(regionText))
                errorMessage = $"Выберите регион";

            CheckPanel1Properties(errorMessage, nameof(updateRequestDto.Country.Region), ref RegionIconColor);
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

            if (result != null && result.Canceled == false && updateRequestDto.Users.Contains(user))
            {
                var index = updateRequestDto.Users.IndexOf(user);
                if (index >= 0)
                    updateRequestDto.Users[index].IsDeleted = true;
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
                var position = updateRequestDto.Users.IndexOf(user);
                updateRequestDto.Users.RemoveAt(position);
                updateRequestDto.Users.Insert(position, result.Data.DeepCopy<UsersDto>()!);
            }
        }

        async Task AddUserAsync(MouseEventArgs args)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, null } };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Добавление партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
                updateRequestDto.Users.Add((UsersDto)result.Data);

            CheckPanel2Properties();
        }

        void CheckPanel2Properties() =>
            TabPanels[2].Items[nameof(updateRequestDto.Users)] = updateRequestDto.Users.Where(w => !w.IsDeleted).Count() == 0 ? false : true;
        #endregion


        #region /// ШАГ 3: ХОББИ ///
        void OnHobbyChanged(HobbiesDto hobby)
        {
            if (updateRequestDto.Hobbies != null)
            {
                var index = updateRequestDto.Hobbies.FindIndex(x => x.Id == hobby.Id);
                if (index >= 0)
                    updateRequestDto.Hobbies.RemoveAt(index);
                else
                    updateRequestDto.Hobbies.Add(hobby);
            }
            else
                updateRequestDto.Hobbies = [hobby];

            CheckPanel3Properties();
        }

        void CheckPanel3Properties()
        {
            if (updateRequestDto.Hobbies != null && updateRequestDto.Hobbies.Any())
                TabPanels[3].Items[nameof(updateRequestDto.Hobbies)] = true;
            else
                TabPanels[3].Items[nameof(updateRequestDto.Hobbies)] = false;
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

                if (updateRequestDto.Photos == null)
                    updateRequestDto.Photos = new List<PhotosForAccountsDto>();

                foreach (var photo in browserPhotos)
                {
                    var newPhoto = await photo.Upload<PhotosForAccountsDto>(CurrentState.Account?.Token, _repoUploadPhotoToTemp, accountId: CurrentState.Account!.Id);

                    if (newPhoto != null)
                        updateRequestDto.Photos.Insert(0, newPhoto);

                    StateHasChanged();
                    if (updateRequestDto.Photos.Count(x => x.IsDeleted == false) >= 20) break;
                }

                processingPhoto = false;
                StateHasChanged();
            }
        }

        void UpdateCommentPhoto(PhotosForAccountsDto photo, string comment) =>
            photo.Comment = comment;

        void SetAsAvatarPhoto(PhotosForAccountsDto photo)
        {
            updateRequestDto.Photos?.ForEach(x => x.IsAvatar = false);
            photo.IsAvatar = true;
        }
        #endregion


        async void SubmitAsync()
        {
            updateRequestDto.ErrorMessage = null;
            processingAccount = true;
            StateHasChanged();

            var response = await _repoUpdate.HttpPostAsync(updateRequestDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                updateRequestDto.ErrorMessage = response.Response.ErrorMessage;
            }
            else
            {
                LoginRequestDto loginRequestDto = new LoginRequestDto
                {
                    Email = updateRequestDto.Email,
                    Password = updateRequestDto.Password,
                    Remember = updateRequestDto.Remember
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
                    updateRequestDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                }
            }

            processingAccount = false;
            CurrentState.StateHasChanged();
        }

        public void Dispose()
        {
            if (updateRequestDto?.Photos != null)
                foreach (var photo in updateRequestDto.Photos.Where(w => w.Id == 0))
                    if (Directory.Exists(StaticData.TempPhotosDir + "/" + photo.Guid))
                        Directory.Delete(StaticData.TempPhotosDir + "/" + photo.Guid, true);
        }

    }
}
