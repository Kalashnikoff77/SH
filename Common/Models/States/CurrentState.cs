using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Net;

namespace Common.Models.States
{
    public partial class CurrentState
    {
        /// <summary>
        /// Данные о залогиненном пользователе
        /// </summary>
        public AccountsViewDto? Account { get; set; } = null;

        /// <summary>
        /// Пользователь залогинен? (для быстрой проверки)
        /// </summary>
        public bool IsAccountLoggedIn { get; set; }

        public event Action? OnChange;
        public IJSRuntime JS { get; set; } = null!;

        ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        NavigationManager _navigationManager { get; set; } = null!;

        IJSProcessor _JSProcessor { get; set; } = null!;

        IRepository<AccountReloadModel, AccountReloadRequestDto, AccountReloadResponseDto> _repoReload { get; set; } = null!;

        public CurrentState(ProtectedLocalStorage protectedLocalStore, ProtectedSessionStorage protectedSessionStore,
            NavigationManager navigationManager, IConfiguration configuration, IJSProcessor JSProcessor, IJSRuntime JS,
            IRepository<AccountReloadModel, AccountReloadRequestDto, AccountReloadResponseDto> repoReload)
        {
            _protectedLocalStore = protectedLocalStore;
            _protectedSessionStore = protectedSessionStore;
            _navigationManager = navigationManager;
            _config = configuration;
            _repoReload = repoReload;
            _JSProcessor = JSProcessor;
            this.JS = JS;
        }

        /// <summary>
        /// Вызывает StateHasChanged по всему сайту
        /// </summary>
        public void StateHasChanged() => OnChange?.Invoke();

        /// <summary>
        /// Внесём данные о полученном аккаунте в CurrentState.Account
        /// </summary>
        /// <param name="loggedInAccount"></param>
        public void SetAccount(AccountsViewDto? loggedInAccount)
        {
            Account = loggedInAccount;
            IsAccountLoggedIn = loggedInAccount == null ? false : true;
        }


        /// <summary>
        /// Загружает новые данные о пользователе в CurrentState.Account из базы данных
        /// </summary>
        public async Task ReloadAccountAsync()
        {
            var reloadResponse = await _repoReload.HttpPostAsync(new AccountReloadModel() { Token = Account!.Token });

            if (reloadResponse.StatusCode == HttpStatusCode.OK)
            {
                reloadResponse.Response.Account.Token = Account.Token;
                SetAccount(reloadResponse.Response.Account);
            }
        }

        public async Task LogOutAsync()
        {
            // Снимает статус онлайн с аватаров текущего пользователя
            ConnectedAccounts.Remove(Account!.Id.ToString());
            await _JSProcessor.UpdateOnlineAccountsClient(ConnectedAccounts);

            SetAccount(null);
            await _protectedLocalStore.DeleteAsync(nameof(LoginModel));
            await _protectedSessionStore.DeleteAsync(nameof(LoginModel));
            StateHasChanged();

            await SignalRConnect();

            _navigationManager.NavigateTo("/");
        }
    }
}
