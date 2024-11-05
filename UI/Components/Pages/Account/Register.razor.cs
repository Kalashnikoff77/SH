using Common.Dto;
using Common.Dto.Requests;
using Common.Models;
using Common.Models.States;
using System.Net;
using UI.Models;

namespace UI.Components.Pages.Account
{
    public partial class Register : AccountDto, IDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>()
            {
                { 1, new TabPanel
                    {
                        Items = new Dictionary<string, bool>
                    {
                        { nameof(accountRequestDto.Name), false },
                        { nameof(accountRequestDto.Email), false },
                        { nameof(accountRequestDto.Password), false },
                        { nameof(accountRequestDto.Password2), false },
                        { nameof(accountRequestDto.Country), false },
                        { nameof(accountRequestDto.Country.Region), false }
                    }
                    }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { nameof(accountRequestDto.Users), false } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { nameof(accountRequestDto.Hobbies), false } } } },
                { 4, new TabPanel { Items = new Dictionary<string, bool> { { "Photos", false } } } }
            };

            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries = apiCountriesResponse.Response.Countries;

            var apiHobbiesResponse = await _repoGetHobbies.HttpPostAsync(new GetHobbiesRequestDto());
            hobbies = apiHobbiesResponse.Response.Hobbies;

            // TODO УДАЛИТЬ ЗНАЧЕНИЯ ПО УМОЛЧАНИЮ (OK)
            accountRequestDto = new RegisterAccountRequestDto
            {
                Name = "Олег и Марина Мск2",
                Email = "olegmar@mail.ru",
                Password = "pass1234",
                Password2 = "pass1234",
                Users = new List<UsersDto>
                {
                    new UsersDto { Id = 0, Name = "Олег", Gender = 0, Weight=80, Height=180, BirthDate = DateTime.Parse("29.01.1977") },
                    new UsersDto { Id = 1, Name = "Марина", Gender = 1, Weight=74, Height=173, BirthDate = DateTime.Parse("01.07.1969") }
                }
            };
        }


        async void SubmitAsync()
        {
            accountRequestDto.ErrorMessage = null;
            processingAccount = true;
            StateHasChanged();

            var response = await _repoRegister.HttpPostAsync((RegisterAccountRequestDto)accountRequestDto);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                accountRequestDto.ErrorMessage = response.Response.ErrorMessage;
                processingAccount = false;
                StateHasChanged();
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

                    await _JSProcessor.Redirect("/");
                }
                else
                {
                    accountRequestDto.ErrorMessage = apiResponse.Response.ErrorMessage;
                    StateHasChanged();
                }
            }
        }
    }
}
