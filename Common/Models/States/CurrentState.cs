﻿using Common.Dto.Requests;
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
        public CurrentState(ProtectedLocalStorage protectedLocalStore, ProtectedSessionStorage protectedSessionStore,
            NavigationManager navigationManager, IConfiguration configuration, IJSProcessor JSProcessor, IJSRuntime JS,
            IRepository<AccountReloadRequestDto, AccountReloadResponseDto> repoReload)
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
        /// Данные о залогиненном пользователе
        /// </summary>
        public AccountsViewDto? Account { get; set; } = null;

        /// <summary>
        /// Пользователь залогинен? (для быстрой проверки)
        /// </summary>
        public bool IsAccountLoggedIn { get; set; }

        /// <summary>
        /// Состояние, хранящееся в браузере (обычно параметры)
        /// </summary>
        public BrowserState BrowserState { get; set; } = new BrowserState();

        public event Action? OnChange;
        public IJSRuntime JS { get; set; } = null!;

        ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;
        NavigationManager _navigationManager { get; set; } = null!;

        IJSProcessor _JSProcessor { get; set; } = null!;

        IRepository<AccountReloadRequestDto, AccountReloadResponseDto> _repoReload { get; set; } = null!;

        /// <summary>
        /// Вызывает StateHasChanged по всему сайту
        /// </summary>
        public void StateHasChanged() => OnChange?.Invoke();

        /// <summary>
        /// Внесём данные о полученном аккаунте в CurrentState.Account
        /// </summary>
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
            var reloadResponse = await _repoReload.HttpPostAsync(new AccountReloadRequestDto() { Token = Account!.Token });

            if (reloadResponse.StatusCode == HttpStatusCode.OK)
            {
                reloadResponse.Response.Account.Token = Account.Token;
                SetAccount(reloadResponse.Response.Account);
            }
        }

        public async Task LogOutAsync()
        {
            // Снимает статус онлайн с аватаров текущего пользователя
            if (Account != null)
                ConnectedAccounts.Remove(Account.Id.ToString());

            await _JSProcessor.UpdateOnlineAccountsClient(ConnectedAccounts);

            SetAccount(null);
            await _protectedLocalStore.DeleteAsync(nameof(LoginRequestDto));
            await _protectedSessionStore.DeleteAsync(nameof(LoginRequestDto));
            StateHasChanged();

            await SignalRConnect();

            _navigationManager.NavigateTo("/");
        }
    }
}
