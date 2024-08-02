using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;
using System.Text.Json;
using UI.Components.Shared;

namespace UI.Components.Pages
{
    public partial class AccountSettings
    {
        [Inject] IRepository<AccountUpdateModel, AccountUpdateRequestDto, AccountUpdateResponseDto> _repoUpdate { get; set; } = null!;
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IMapper _mapper { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;

        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        AccountUpdateModel UpdatingModel { get; set; } = null!;

        int selectedIndex = 0;

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }

        protected override void OnParametersSet()
        {
            if (CurrentState.Account != null)
            {
                // Делаем глубокое копирование через JSON.
                UpdatingModel = _mapper.Map<AccountUpdateModel>(CurrentState.Account);
                var json = JsonSerializer.Serialize(UpdatingModel);
                UpdatingModel = JsonSerializer.Deserialize<AccountUpdateModel>(json)!;

                regions = countries
                    .Where(x => x.Id == UpdatingModel.Country.Id).FirstOrDefault()?
                    .Regions?.Select(s => s).ToList();
            }
        }

        #region /// ШАГ 1: ОБЩЕЕ ///
        int countryId
        {
            get { return UpdatingModel.Country.Id; }
            set
            {
                UpdatingModel.Country.Id = value;
                regions = countries
                    .Where(x => x.Id == countryId).FirstOrDefault()?
                    .Regions?.Select(s => s).ToList();
            }
        }

        int regionId
        {
            get { return UpdatingModel.Country.Region.Id; }
            set { UpdatingModel.Country.Region.Id = value; }
        }
        #endregion

        #region /// ШАГ 2: ПАРТНЁРЫ ///
        RadzenDataGrid<UsersDto> usersGrid = null!;

        async Task OpenEditUserForm(UsersDto? user)
        {
            var newUser = await DialogService.OpenAsync<EditUserForm>($"Новый партнёр для {UpdatingModel.Name}",
                  new Dictionary<string, object?>() { { "User", user } },
                  new DialogOptions() { Width = "500px", Height = "450px" });

            if (newUser != null)
            {
                // Если редактируем пользователя, то удалим старого и добавим как нового
                if (user != null && UpdatingModel.Users.Contains(user))
                    UpdatingModel.Users.Remove(user);

                newUser.Id = 0;
                UpdatingModel.Users.Add(newUser);
                await usersGrid.InsertRow(newUser);
                await usersGrid.Reload();
            }
        }

        async Task DeleteRow(UsersDto user)
        {
            if (UpdatingModel.Users.Contains(user))
                UpdatingModel.Users.Remove(user);
            await usersGrid.Reload();
        }

        #endregion
    }
}
