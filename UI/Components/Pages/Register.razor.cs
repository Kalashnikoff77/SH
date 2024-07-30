using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System.Text.Json;
using UI.Components.Shared;

namespace UI.Components.Pages
{
    public partial class Register
    {
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;

        RadzenDataGrid<UsersDto> usersGrid = null!;
        AccountRegisterModel RegisterModel = new AccountRegisterModel();
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }

        // ШАГ 1: ОБЩЕЕ
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


        // ШАГ 2: АВАТАР
        int progressUpload;
        bool cancelUpload;

        void TrackProgress(UploadProgressArgs args)
        {
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
            progressUpload = 0;
        }


        // ШАГ 3: ПАРТНЁРЫ
        bool IsRegisterButtonDisabled = true;
        bool IsNewUserButtonDisabled = false;

        async Task OpenEditUserForm(int? userId)
        {
            var newUser = await DialogService.OpenAsync<EditUserForm>($"Новый партнёр для {RegisterModel.Name}",
                  new Dictionary<string, object?>() { { "User", null } },
                  new DialogOptions() { Width = "500px", Height = "450px" });

            if (newUser != null)
            {
                RegisterModel.Users.Add(newUser);
                await usersGrid.InsertRow(newUser);
                IsRegisterButtonDisabled = false;
                IsNewUserButtonDisabled = RegisterModel.Users.Count >= 4 ? true : false;
            }
        }

        async Task DeleteRow(UsersDto user)
        {
            if (RegisterModel.Users.Contains(user))
            {
                RegisterModel.Users.Remove(user);
                await usersGrid.Reload();
            }
            else
            {
                await usersGrid.Reload();
            }
            IsRegisterButtonDisabled = RegisterModel.Users.Count > 0 ? false : true;
            IsNewUserButtonDisabled = RegisterModel.Users.Count >= 4 ? true : false;
        }

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
    }
}
