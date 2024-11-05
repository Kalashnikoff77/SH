using Common.Dto.Requests;
using Common.Models;
using Common.Models.States;
using System.Net;
using UI.Models;

namespace UI.Components.Pages.Account
{
    public partial class Profile : AccountDto, IDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>() 
            {
                { 1, new TabPanel
                    {
                        Items = new Dictionary<string, bool>
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


        async void SubmitAsync()
        {
            accountRequestDto.ErrorMessage = null;
            processingAccount = true;
            StateHasChanged();

            var response = await _repoUpdate.HttpPostAsync((UpdateAccountRequestDto)accountRequestDto);

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
    }
}
