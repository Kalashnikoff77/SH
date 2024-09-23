using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Models.States;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Layout
{
    public partial class MainLayout : IAsyncDisposable
    {
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        [Inject] CurrentState CurrentState { get; set; } = null!;
        [Inject] IConfiguration _configuration { get; set; } = null!;

        [Inject] IRepository<LoginRequestDto, LoginResponseDto> _repoLogin { get; set; } = null!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var storage = await _protectedLocalStore.GetAsync<LoginRequestDto>(nameof(LoginRequestDto));
                if (!storage.Success)
                    storage = await _protectedSessionStore.GetAsync<LoginRequestDto>(nameof(LoginRequestDto));

                if (storage.Success && storage.Value != null)
                {
                    var apiResponse = await _repoLogin.HttpPostAsync(storage.Value);
                    if (apiResponse.Response.Account != null)
                    {
                        apiResponse.Response.Account!.Token = StaticData.GenerateToken(apiResponse.Response.Account.Id, apiResponse.Response.Account.Guid, _configuration);
                        CurrentState.SetAccount(apiResponse.Response.Account);
                        CurrentState.StateHasChanged();
                    }
                    // Если пользователя из базы удалили, но он залогинен, то сделаем принудительный выход
                    else
                    {
                        await CurrentState.LogOutAsync();
                    }
                }

                await CurrentState.SignalRConnect();
            }
        }

        public async ValueTask DisposeAsync()
        {
            CurrentState.OnChange -= StateHasChanged;
            await CurrentState.SignalRDisconnect();
        }
    }
}
