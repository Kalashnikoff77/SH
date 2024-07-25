using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.JSProcessor;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using System.Net;

namespace UI.Components.Pages
{
    public partial class Login
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<LoginModel, LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IConfiguration _configuration { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] NavigationManager Navigation { get; set; } = null!;

        // TODO REMOVE (OK)
        string userName = "oleg@mail.ru";
        string password = "pass2";
        bool rememberMe = true;

        string? errorLogin { get; set; } = null;

        async void OnLogin(LoginArgs args)
        {
            LoginModel loginModel = new LoginModel
            {
                Email = args.Username,
                Password = args.Password,
                Remember = args.RememberMe
            };

            var apiResponse = await _repoLogin.HttpPostAsync(loginModel);
            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                apiResponse.Response.Account!.Token = Common.StaticData.GenerateToken(apiResponse.Response.Account.Id, apiResponse.Response.Account.Guid, _configuration);
                CurrentState.SetAccount(apiResponse.Response.Account);

                if (loginModel.Remember)
                    await _protectedLocalStore.SetAsync(nameof(LoginModel), loginModel);
                else
                    await _protectedSessionStore.SetAsync(nameof(LoginModel), loginModel);

                await _JSProcessor.Redirect("/");
            }
            else
            {
                errorLogin = apiResponse.Response.ErrorMessage;
                StateHasChanged();
            }
        }

        void OnRegister()
        {
            Navigation.NavigateTo("/register");
        }

        void OnResetPassword(string email)
        {
        }
    }
}
