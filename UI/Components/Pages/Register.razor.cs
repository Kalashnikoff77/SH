using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

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


        private void OnChange()
        {
        }


        private void CanChange(StepsCanChangeEventArgs args)
        {
        }

    }
}
