using Common.Dto.Requests;
using Common.Extensions;
using Common.Models;
using Common.Models.States;
using MudBlazor;
using System.Net;
using UI.Models;

namespace UI.Components.Pages.Account
{
    public partial class Profile : AccountDtoBase, IDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>() 
            {
                { 1, new TabPanel
                    {
                        Items = new Dictionary<string, bool>
                        {
                            { nameof(AccountRequestDto.Name), true },
                            { nameof(AccountRequestDto.Email), true },
                            { nameof(AccountRequestDto.Password), true },
                            { nameof(AccountRequestDto.Password2), true },
                            { nameof(AccountRequestDto.Country), true },
                            { nameof(AccountRequestDto.Country.Region), true }
                        }
                    }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { nameof(AccountRequestDto.Users), true } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { nameof(AccountRequestDto.Hobbies), true } } } },
                { 4, new TabPanel { Items = new Dictionary<string, bool> { { "Photos", true } } } }
            };

            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries = apiCountriesResponse.Response.Countries;

            var apiHobbiesResponse = await _repoGetHobbies.HttpPostAsync(new GetHobbiesRequestDto());
            Hobbies = apiHobbiesResponse.Response.Hobbies;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null && isFirstSetParameters)
            {
                AccountRequestDto = CurrentState.Account.DeepCopy<UpdateAccountRequestDto>()!;
                AccountRequestDto.Password2 = AccountRequestDto.Password; // Дубликат пароля для формы

                countryText = CurrentState.Account.Country!.Name;
                regionText = CurrentState.Account.Country!.Region.Name;

                var storage = await _protectedLocalStore.GetAsync<LoginRequestDto>(nameof(LoginRequestDto));
                if (storage.Success)
                    AccountRequestDto.Remember = true;

                isFirstSetParameters = false;
            }

            NameIconColor = EmailIconColor = PasswordIconColor = Password2IconColor = CountryIconColor = RegionIconColor = Color.Success;
        }


        async void SubmitAsync()
        {
            AccountRequestDto.ErrorMessage = null;
            ProcessingAccount = true;
            StateHasChanged();

            var response = await _repoUpdate.HttpPostAsync((UpdateAccountRequestDto)AccountRequestDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                AccountRequestDto.ErrorMessage = response.Response.ErrorMessage;
            }
            else
            {
                LoginRequestDto loginRequestDto = new LoginRequestDto
                {
                    Email = AccountRequestDto.Email,
                    Password = AccountRequestDto.Password,
                    Remember = AccountRequestDto.Remember
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

                    IsDataSaved = true;
                }
                else
                {
                    AccountRequestDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                }
            }

            ProcessingAccount = false;
            CurrentState.StateHasChanged();
        }
    }
}
