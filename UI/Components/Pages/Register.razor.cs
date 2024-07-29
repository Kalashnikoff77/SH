using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using UI.Components.Shared;

namespace UI.Components.Pages
{
    public partial class Register
    {
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGet { get; set; } = null!;

        AccountRegisterModel RegisterModel = new AccountRegisterModel();
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGet.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }


        // ШАГ 2
        async Task OpenEditUserForm(int? userId)
        {
            var newUser = await DialogService.OpenAsync<EditUserForm>($"Новый партнёр для {RegisterModel.Name}",
                  new Dictionary<string, object?>() { { "User", null } },
                  new DialogOptions() { Width = "500px", Height = "450px" });

            if (newUser != null)
            {
                RegisterModel.Users.Add(newUser);
                await usersGrid.InsertRow(newUser);
            }
        }


        private void OnChange()
        {
        }


        private void CanChange(StepsCanChangeEventArgs args)
        {
        }

    }
}
