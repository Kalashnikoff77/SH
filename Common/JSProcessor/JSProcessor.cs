﻿using Microsoft.JSInterop;

namespace Common.JSProcessor
{
    public class JSProcessor : IJSProcessor
    {
        IJSRuntime _JS { get; set; }

        public JSProcessor(IJSRuntime JS) => _JS = JS;

        public async Task ChangeNumberFadeInOut(string tagClass, int? number, bool isShowZero = false) => 
            await RunJSAsync(nameof(ChangeNumberFadeInOut), tagClass, number, isShowZero);

        public async Task ChangeNumberInButtonsFadeInOut(string tagClass, int? number) =>
            await RunJSAsync(nameof(ChangeNumberInButtonsFadeInOut), tagClass, number);

        public async Task Redirect(string url) =>
            await RunJSAsync(nameof(Redirect), url);

        public async Task ScrollDivToBottom(string divId) =>
            await RunJSAsync(nameof(ScrollDivToBottom), divId);

        public async Task ScrollToElement(string elementId) =>
            await RunJSAsync(nameof(ScrollToElement), elementId);

        public async Task UpdateOnlineAccountsClient(HashSet<string> ConnectedAccounts) =>
            await RunJSAsync(nameof(UpdateOnlineAccountsClient), ConnectedAccounts);

        async Task RunJSAsync(string identifier, params object?[] args)
        {
            try
            {
                await _JS.InvokeVoidAsync(identifier, args);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
