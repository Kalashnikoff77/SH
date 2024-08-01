using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;
using System.Net;
using System.Text.Json;
using UI.Components.Shared;

namespace UI.Components.Pages
{
    public partial class Register
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<AccountRegisterModel, AccountRegisterRequestDto, ResponseDtoBase> _repoRegister { get; set; } = null!;
        [Inject] IRepository<LoginModel, LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        RadzenDataGrid<UsersDto> usersGrid = null!;
        AccountRegisterModel RegisterModel = new AccountRegisterModel();
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }

        #region /// ШАГ 1: ОБЩЕЕ ///
        int countryId
        {
            get { return RegisterModel.Country.Id; }
            set
            {
                RegisterModel.Country.Id = value;
                regions = countries?
                    .Where(x => x.Id == countryId).FirstOrDefault()?
                    .Regions?.Select(s => s).ToList();
            }
        }

        int regionId
        {
            get { return RegisterModel.Country.Region.Id; }
            set { RegisterModel.Country.Region.Id = value; }
        }
        #endregion


        #region /// ШАГ 2: АВАТАР ///
        int progressUpload;
        bool cancelUpload;
        UploadErrorEventArgs errorUploadText = new UploadErrorEventArgs();
        class ErrorUpload { public string ErrorMessage { get; set; } = null!; }

        void OnProgress(UploadProgressArgs args)
        {
            RegisterModel.ErrorUploadMessage = null;
            progressUpload = args.Progress;
            args.Cancel = cancelUpload;
        }

        void OnComplete(UploadCompleteEventArgs args)
        {
            var response = JsonSerializer.Deserialize<UploadTempFileResponseDto>(args.RawResponse);
            if (response != null)
            {
                RegisterModel.OriginalPhoto = response.originalFileName;
                RegisterModel.PreviewPhoto = response.previewFileName;
            }
        }

        void OnError(UploadErrorEventArgs args)
        {
            var response = JsonSerializer.Deserialize<ErrorUpload>(args.Message);
            if (response != null)
                RegisterModel.ErrorUploadMessage = response.ErrorMessage;
        }
        #endregion


        #region /// ШАГ 3: ПАРТНЁРЫ ///
        async Task OpenEditUserForm(UsersDto? user)
        {
            var newUser = await DialogService.OpenAsync<EditUserForm>($"Новый партнёр для {RegisterModel.Name}",
                  new Dictionary<string, object?>() { { "User", user } },
                  new DialogOptions() { Width = "500px", Height = "450px" });

            if (newUser != null)
            {
                // Если редактируем пользователя, то удалим старого и добавим как нового
                if (user != null && RegisterModel.Users.Contains(user))
                    RegisterModel.Users.Remove(user);

                newUser.Id = 0;
                RegisterModel.Users.Add(newUser);
                await usersGrid.InsertRow(newUser);
                await usersGrid.Reload();
            }
        }

        async Task DeleteRow(UsersDto user)
        {
            if (RegisterModel.Users.Contains(user))
                RegisterModel.Users.Remove(user);
            await usersGrid.Reload();
        }
        #endregion

        private void CanChange(StepsCanChangeEventArgs args)
        {
            if (args.SelectedIndex == 0)
            {
                if (string.IsNullOrWhiteSpace(RegisterModel.Name) ||
                    string.IsNullOrWhiteSpace(RegisterModel.Email) ||
                    string.IsNullOrWhiteSpace(RegisterModel.Password) ||
                    string.IsNullOrWhiteSpace(RegisterModel.Password2) ||
                    (RegisterModel.Country.Region.Id == 0))
                {
                    args.PreventDefault();
                    return;
                }
            }

            if (args.SelectedIndex == 1 && args.NewIndex == 2)
            {
                if (string.IsNullOrWhiteSpace(RegisterModel.PreviewPhoto))
                {
                    args.PreventDefault();
                    return;
                }
            }
        }

        private async void RegistrationAsync()
        {
            RegisterModel.ErrorRegisterMessage = null;

            var response = await _repoRegister.HttpPostAsync(RegisterModel);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                RegisterModel.ErrorRegisterMessage = response.Response.ErrorMessage;
                StateHasChanged();
            } 
            else
            {
                LoginModel loginModel = new LoginModel
                {
                    Email = RegisterModel.Email,
                    Password = RegisterModel.Password,
                    Remember = RegisterModel.RememberMe
                };

                var apiResponse = await _repoLogin.HttpPostAsync(loginModel);

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse.Response.Account!.Token = Common.StaticData.GenerateToken(apiResponse.Response.Account.Id, apiResponse.Response.Account.Guid, _config);
                    CurrentState.SetAccount(apiResponse.Response.Account);

                    if (loginModel.Remember)
                        await _protectedLocalStore.SetAsync(nameof(LoginModel), loginModel);
                    else
                        await _protectedSessionStore.SetAsync(nameof(LoginModel), loginModel);

                    await _JSProcessor.Redirect("/");
                }
                else
                {
                    RegisterModel.ErrorRegisterMessage = apiResponse.Response.ErrorMessage;
                    StateHasChanged();
                }
            }
        }
    }
}
