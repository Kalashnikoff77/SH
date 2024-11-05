using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.JSProcessor;
using Common.Models.States;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using System.Net;
using MudBlazor;

namespace UI.Components.Pages.Account
{
    public partial class Login
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] IConfiguration _configuration { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] NavigationManager Navigation { get; set; } = null!;

        // TODO Убрать начальные значения (OK)
        LoginRequestDto loginRequestDto = new LoginRequestDto
        {
            Email = "oleg@mail.ru",
            Password = "pass2",
            Remember = true
        };

        string? errorLogin { get; set; } = null;

        async void OnLoginAsync()
        {
            var apiResponse = await _repoLogin.HttpPostAsync(loginRequestDto);
            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                apiResponse.Response.Account!.Token = StaticData.GenerateToken(apiResponse.Response.Account.Id, apiResponse.Response.Account.Guid, _configuration);
                CurrentState.SetAccount(apiResponse.Response.Account);

                if (loginRequestDto.Remember)
                    await _protectedLocalStore.SetAsync(nameof(LoginRequestDto), loginRequestDto);
                else
                    await _protectedSessionStore.SetAsync(nameof(LoginRequestDto), loginRequestDto);

                await _JSProcessor.Redirect("/");
            }
            else
            {
                errorLogin = apiResponse.Response.ErrorMessage;
                StateHasChanged();
            }
        }

        void OnResetPassword(string email)
        {
        }


        Color EmailIconColor = Color.Default;
        string? EmailValidator(string email)
        {
            string? errorMessage = null;
            EmailIconColor = Color.Success;

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = $"Укажите email";
                EmailIconColor = Color.Error;
            }

            if (string.IsNullOrWhiteSpace(email) || email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN)
            {
                errorMessage = $"Email может содержать {StaticData.DB_ACCOUNTS_EMAIL_MIN}-{StaticData.DB_ACCOUNTS_EMAIL_MAX} символов";
                EmailIconColor = Color.Error;
            }

            StateHasChanged();
            return errorMessage;
        }

        Color PasswordIconColor = Color.Default;
        string? PasswordValidator(string pass)
        {
            string? errorMessage = null;
            PasswordIconColor = Color.Success;

            if (string.IsNullOrWhiteSpace(pass))
            {
                errorMessage = $"Введите пароль";
                PasswordIconColor = Color.Error;
            }

            if (string.IsNullOrWhiteSpace(pass) || pass.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN)
            {
                errorMessage = $"Пароль может содержать {StaticData.DB_ACCOUNTS_PASSWORD_MIN}-{StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов";
                PasswordIconColor = Color.Error;
            }

            StateHasChanged();
            return errorMessage;
        }

    }
}
