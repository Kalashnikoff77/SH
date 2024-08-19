using Common;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Enums;
using Common.JSProcessor;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using PhotoSauce.MagicScaler;
using System.Net;
using System.Text.RegularExpressions;
using UI.Components.Shared;
using UI.Extensions;
using UI.Models;

namespace UI.Components.Pages
{
    public partial class Register : IDisposable
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<AccountCheckRegisterModel, AccountCheckRegisterRequestDto, AccountCheckRegisterResponseDto> _repoCheckRegister { get; set; } = null!;
        [Inject] IRepository<AccountRegisterModel, AccountRegisterRequestDto, ResponseDtoBase> _repoRegister { get; set; } = null!;
        [Inject] IRepository<LoginModel, LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;

        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] IDialogService DialogService { get; set; } = null!;
        [Inject] IConfiguration _config { get; set; } = null!;

        AccountRegisterModel registerModel = new AccountRegisterModel();
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool processingPhoto = false;

        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value.IsValid == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value.IsValid == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value.IsValid == true);

        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, TabPanelItem>
                        {
                            { nameof(registerModel.Name), new TabPanelItem() },
                            { nameof(registerModel.Email), new TabPanelItem() },
                            { nameof(registerModel.Password), new TabPanelItem() },
                            { nameof(registerModel.Password2), new TabPanelItem() },
                            { nameof(registerModel.Country), new TabPanelItem() },
                            { nameof(registerModel.Country.Region), new TabPanelItem() }
                        }
                    }
                },
                { 2, new TabPanel { Items = new Dictionary<string, TabPanelItem> { { nameof(registerModel.Users), new TabPanelItem() } } } },
                { 3, new TabPanel { Items = new Dictionary<string, TabPanelItem> { { nameof(registerModel.Avatar), new TabPanelItem() } } } }
            };

            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);

            // TODO УДАЛИТЬ (OK)
            registerModel.Users = new List<UsersDto> 
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

        Color NameIconColor = Color.Default;
        async Task<string?> NameValidator(string name)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_ACCOUNTS_NAME_MIN)
                errorMessage = $"Имя должно содержать {StaticData.DB_ACCOUNTS_NAME_MIN}-{StaticData.DB_ACCOUNTS_NAME_MAX} символов";

            var apiResponse = await _repoCheckRegister.HttpPostAsync(new AccountCheckRegisterModel { AccountName = name });
            if (apiResponse.Response.AccountNameExists)
                errorMessage = $"Это имя уже занято. Выберите другое.";

            CheckPanel1Properties(errorMessage, nameof(registerModel.Name), ref NameIconColor);

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

            var apiResponse = await _repoCheckRegister.HttpPostAsync(new AccountCheckRegisterModel { AccountEmail = email });
            if (apiResponse.Response.AccountEmailExists)
                errorMessage = $"Этот email уже зарегистрирован. Забыли пароль?";

            CheckPanel1Properties(errorMessage, nameof(registerModel.Email), ref EmailIconColor);

            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string password)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
                errorMessage = $"Пароль должен содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";

            CheckPanel1Properties(errorMessage, nameof(registerModel.Password), ref PasswordIconColor);
            return errorMessage;
        }

        Color Password2IconColor = Color.Default;
        string? Password2Validator(string password2)
        {
            string? errorMessage = null;
            if (registerModel.Password != password2) 
                errorMessage = $"Пароли не совпадают";

            CheckPanel1Properties(errorMessage, nameof(registerModel.Password2), ref Password2IconColor);
            return errorMessage;
        }

        Color CountryIconColor = Color.Default;
        string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText)) 
                errorMessage = $"Выберите страну";

            CheckPanel1Properties(errorMessage, nameof(registerModel.Country), ref CountryIconColor);
            return errorMessage;
        }

        Color RegionIconColor = Color.Default;
        string? RegionValidator(string region)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(regionText))
                errorMessage = $"Выберите регион";

            CheckPanel1Properties(errorMessage, nameof(registerModel.Country.Region), ref RegionIconColor);
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
            
            if (result != null && result.Canceled == false && registerModel.Users.Contains(user))
                registerModel.Users.Remove(user);

            CheckPanel2Properties();
        }

        async Task AddUserAsync(MouseEventArgs args)
        {
            var parameters = new DialogParameters<EditUserDialog> { { x => x.User, null } };
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var resultDialog = await DialogService.ShowAsync<EditUserDialog>("Добавление партнёра", parameters, options);
            var result = await resultDialog.Result;
            if (result != null && result.Canceled == false && result.Data != null)
                registerModel.Users.Add((UsersDto)result.Data);

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
                var position = registerModel.Users.IndexOf(user);
                registerModel.Users.RemoveAt(position);
                registerModel.Users.Insert(position, result.Data.DeepCopy<UsersDto>()!);
            }
        }

        void CheckPanel2Properties() =>
            TabPanels[2].Items[nameof(registerModel.Users)].IsValid = registerModel.Users.Count == 0 ? false : true;
        #endregion


        #region /// ШАГ 3: АВАТАР ///
        const string dir = "../UI/wwwroot/images/AccountsPhotos/temp/";
        string? baseFileName;
        string? originalFileName;
        string? previewFileName;
        string? urlPreviewImage;

        async void UploadAvatar(IBrowserFile file)
        {
            processingPhoto = true;

            if (File.Exists(dir + originalFileName))
                File.Delete(dir + originalFileName);
            if (File.Exists(dir + previewFileName))
                File.Delete(dir + previewFileName);

            baseFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + Guid.NewGuid().ToString();
            originalFileName = baseFileName + Path.GetExtension(file.Name);
            previewFileName = baseFileName + "_" + EnumImageSize.s150x150 + ".jpg";

            await using (FileStream fs = new(dir + originalFileName, FileMode.Create))
                await file.OpenReadStream(file.Size).CopyToAsync(fs);

            using (MemoryStream output = new MemoryStream(500000))
            {
                MagicImageProcessor.ProcessImage(dir + originalFileName, output, StaticData.Images[EnumImageSize.s150x150]);
                await File.WriteAllBytesAsync(dir + previewFileName, output.ToArray());
            }

            urlPreviewImage = previewFileName;
            registerModel.OriginalPhoto = originalFileName;

            TabPanels[3].Items[nameof(registerModel.Avatar)].IsValid = true;

            processingPhoto = false;
            StateHasChanged();
        }
        #endregion


        async void SubmitAsync()
        {
            var response = await _repoRegister.HttpPostAsync(registerModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                registerModel.ErrorRegisterMessage = response.Response.ErrorMessage;
                StateHasChanged();
            }
            else
            {
                LoginModel loginModel = new LoginModel
                {
                    Email = registerModel.Email,
                    Password = registerModel.Password,
                    Remember = registerModel.RememberMe
                };

                var apiResponse = await _repoLogin.HttpPostAsync(loginModel);

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse.Response.Account!.Token = StaticData.GenerateToken(apiResponse.Response.Account.Id, apiResponse.Response.Account.Guid, _config);
                    CurrentState.SetAccount(apiResponse.Response.Account);

                    if (loginModel.Remember)
                        await _protectedLocalStore.SetAsync(nameof(LoginModel), loginModel);
                    else
                        await _protectedSessionStore.SetAsync(nameof(LoginModel), loginModel);

                    await _JSProcessor.Redirect("/");
                }
                else
                {
                    registerModel.ErrorRegisterMessage = apiResponse.Response.ErrorMessage;
                    StateHasChanged();
                }
            }

        }

        public void Dispose()
        {
            if (File.Exists(dir + originalFileName))
                File.Delete(dir + originalFileName);
            if (File.Exists(dir + previewFileName))
                File.Delete(dir + previewFileName);
        }
    }
}
