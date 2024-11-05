using AutoMapper;
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
using System.Text.RegularExpressions;
using UI.Components.Dialogs;
using UI.Models;

namespace UI.Components.Pages.Account
{
    public abstract class AccountDto : ComponentBase
    {
        [CascadingParameter] protected CurrentState CurrentState { get; set; } = null!;
        [Inject] protected IRepository<UploadPhotoToTempRequestDto, UploadPhotoToTempResponseDto> _repoUploadPhotoToTemp { get; set; } = null!;
        [Inject] protected IRepository<GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] protected IRepository<GetHobbiesRequestDto, GetHobbiesResponseDto> _repoGetHobbies { get; set; } = null!;
        [Inject] protected IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;

        [Inject] protected ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] protected ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] protected IDialogService _dialogService { get; set; } = null!;
        [Inject] protected IConfiguration _config { get; set; } = null!;
        [Inject] protected IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] protected IMapper _mapper { get; set; } = null!;

        [Inject] protected IRepository<AccountCheckUpdateRequestDto, AccountCheckUpdateResponseDto> _repoCheckUpdate { get; set; } = null!;
        [Inject] protected IRepository<UpdateAccountRequestDto, ResponseDtoBase> _repoUpdate { get; set; } = null!;
        [Inject] protected IRepository<AccountCheckRegisterRequestDto, AccountCheckRegisterResponseDto> _repoCheckRegister { get; set; } = null!;
        [Inject] protected IRepository<RegisterAccountRequestDto, ResponseDtoBase> _repoRegister { get; set; } = null!;

        protected bool processingAccount, processingPhoto, isDataSaved = false;
        /// <summary>
        /// Для предотвращения повторного выполнения OnParametersSet (выполняется при переходе на другую ссылку)
        /// </summary>
        protected bool isFirstSetParameters = true;

        protected List<CountriesViewDto> countries { get; set; } = null!;
        protected List<RegionsDto>? regions { get; set; } = null;
        protected List<HobbiesDto> hobbies { get; set; } = null!;

        protected AccountRequestDtoBase accountRequestDto { get; set; } = null!;

        protected Dictionary<short, TabPanel> TabPanels = new Dictionary<short, TabPanel>();

        protected bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value == true);
        protected bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value == true);
        protected bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value == true);
        protected bool IsPanel4Valid => TabPanels[4].Items.All(x => x.Value == true);


        #region /// ШАГ 1: ОБЩЕЕ ///
        string? _countryText;
        protected string? countryText
        {
            get => _countryText;
            set
            {
                if (value != null && countries.Any())
                {
                    var country = countries.Where(c => c.Name == value).FirstOrDefault();
                    if (country != null)
                    {
                        accountRequestDto.Country.Id = country.Id;
                        regions = country.Regions;
                    }
                }
                _countryText = value;
                _regionText = null;
            }
        }

        string? _regionText;
        protected string? regionText
        {
            get => _regionText;
            set
            {
                if (value != null && countries.Any() && regions != null)
                {
                    var region = regions.Where(c => c.Name == value).FirstOrDefault();
                    if (region != null)
                        accountRequestDto.Country.Region.Id = region.Id;
                }
                _regionText = value;
            }
        }

        protected Color NameIconColor = Color.Default;
        protected async Task<string?> NameValidator(string name)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_ACCOUNTS_NAME_MIN)
                errorMessage = $"Имя должно содержать {StaticData.DB_ACCOUNTS_NAME_MIN}-{StaticData.DB_ACCOUNTS_NAME_MAX} символов";

            var apiResponse = await _repoCheckUpdate.HttpPostAsync(new AccountCheckUpdateRequestDto { AccountName = name, Token = CurrentState.Account?.Token });
            if (apiResponse.Response.AccountNameExists)
                errorMessage = $"Это имя уже занято. Выберите другое.";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Name), ref NameIconColor);

            return errorMessage;
        }

        protected Color EmailIconColor = Color.Default;
        protected async Task<string?> EmailValidator(string email)
        {
            string? errorMessage = null;

            if (string.IsNullOrWhiteSpace(email) || email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN)
                errorMessage = $"Email может содержать {StaticData.DB_ACCOUNTS_EMAIL_MIN}-{StaticData.DB_ACCOUNTS_EMAIL_MAX} символов";

            if (!Regex.IsMatch(email, @"^[a-z0-9_\.-]{1,32}@[a-z0-9\.-]{1,32}\.[a-z]{2,8}$"))
                errorMessage = $"Проверьте корректность email";

            var apiResponse = await _repoCheckUpdate.HttpPostAsync(new AccountCheckUpdateRequestDto { AccountEmail = email, Token = CurrentState.Account?.Token });
            if (apiResponse.Response.AccountEmailExists)
                errorMessage = $"Этот email уже занят другим пользователем.";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Email), ref EmailIconColor);

            return errorMessage;
        }

        protected Color PasswordIconColor = Color.Default;
        protected string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль должен содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Password), ref PasswordIconColor);
            return errorMessage;
        }

        protected Color Password2IconColor = Color.Default;
        protected string? Password2Validator(string password2)
        {
            string? errorMessage = null;
            if (accountRequestDto.Password != password2)
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Password2), ref Password2IconColor);
            return errorMessage;
        }

        protected Color CountryIconColor = Color.Default;
        protected string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText))
                errorMessage = $"Выберите страну";

            // Сбросим в false регион
            TabPanels[1].Items[nameof(accountRequestDto.Country.Region)] = false;
            RegionIconColor = Color.Default;

            CheckPanel1Properties(errorMessage, nameof(accountRequestDto.Country), ref CountryIconColor);
            return errorMessage;
        }

        protected Color RegionIconColor = Color.Default;
        protected string? RegionValidator(string region)
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


        protected async Task<IEnumerable<string>?> SearchCountryAsync(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
                return countries.Select(s => s.Name);

            return countries?.Select(s => s.Name)
                .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        protected async Task<IEnumerable<string>?> SearchRegionAsync(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
                return regions?.Select(s => s.Name);

            return regions?.Select(s => s.Name)
                .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }
        #endregion


        #region /// ШАГ 2: ПАРТНЁРЫ ///
        protected async Task DeleteUserDialogAsync(UsersDto user)
        {
            var parameters = new DialogParameters<ConfirmDialog>
            {
                { x => x.ContentText, $"Удалить партнёра {user.Name}?" },
                { x => x.ButtonText, "Удалить" },
                { x => x.Color, Color.Error }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var resultDialog = await _dialogService.ShowAsync<ConfirmDialog>($"Удаление {user.Name}", parameters, options);
            var result = await resultDialog.Result;

            if (result != null && result.Canceled == false && accountRequestDto.Users.Contains(user))
                accountRequestDto.Users.Remove(user);

            CheckPanel2Properties();
        }

        protected async Task AddUserDialogAsync(MouseEventArgs args)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, null } };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await _dialogService.ShowAsync<EditUserDialog>("Добавление партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
                accountRequestDto.Users.Add((UsersDto)result.Data);

            CheckPanel2Properties();
        }

        protected async Task UpdateUserDialogAsync(UsersDto user)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, user } };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await _dialogService.ShowAsync<EditUserDialog>("Редактирование партнёра", parameters, options);
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
        protected void OnHobbyChanged(HobbiesDto hobby)
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
        protected async void UploadPhotosAsync(IReadOnlyList<IBrowserFile> browserPhotos)
        {
            if (browserPhotos.Count > 0)
            {
                processingPhoto = true;
                StateHasChanged();

                if (accountRequestDto.Photos == null)
                    accountRequestDto.Photos = new List<PhotosForAccountsDto>();

                foreach (var photo in browserPhotos)
                {
                    var newPhoto = await photo.Upload<PhotosForAccountsDto>(CurrentState.Account?.Token, _repoUploadPhotoToTemp, accountId: CurrentState.Account?.Id ?? 0);

                    if (newPhoto != null)
                    {
                        // Если это первая фотка, то отметим её как аватар
                        if (accountRequestDto.Photos.Count(x => x.IsDeleted == false) == 0)
                            newPhoto.IsAvatar = true;
                        accountRequestDto.Photos.Insert(0, newPhoto);
                    }
                    if (accountRequestDto.Photos.Count(x => x.IsDeleted == false) >= 20) break;
                }

                processingPhoto = false;

                CheckPanel4Properties();
            }
        }

        void CheckPanel4Properties()
        {
            if (accountRequestDto.Photos.Count(x => x.IsDeleted == false) > 0)
                TabPanels[4].Items["Photos"] = true;
            else
                TabPanels[4].Items["Photos"] = false;

            StateHasChanged();
        }

        protected void UpdateCommentPhoto(PhotosForAccountsDto photo, string comment) =>
            photo.Comment = comment;

        protected void SetAsAvatarPhoto(PhotosForAccountsDto photo)
        {
            accountRequestDto.Photos?.ForEach(x => x.IsAvatar = false);
            photo.IsAvatar = true;
        }
        #endregion


        public void Dispose()
        {
            if (accountRequestDto?.Photos != null)
                foreach (var photo in accountRequestDto.Photos.Where(w => w.Id == 0))
                    if (Directory.Exists(StaticData.TempPhotosDir + "/" + photo.Guid))
                        Directory.Delete(StaticData.TempPhotosDir + "/" + photo.Guid, true);
        }
    }
}
