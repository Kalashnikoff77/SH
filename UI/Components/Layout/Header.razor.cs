using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Enums;
using Common.JSProcessor;
using Common.Models;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using UI.Components.Layout.Popup;

namespace UI.Components.Layout
{
    public partial class Header
    {
        [Inject] IRepository<GetNotificationsCountModel, GetNotificationsCountRequestDto, GetNotificationsCountResponseDto> _repoNotifCount { get; set; } = null!;
        [Inject] IRepository<GetMessagesCountModel, GetMessagesCountRequestDto, GetMessagesCountResponseDto> _repoCount { get; set; } = null!;
        [Inject] IRepository<GetLastMessagesListModel, GetLastMessagesListRequestDto, GetLastMessagesListResponseDto> _repoMessages { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        IDisposable? updateNotificationsCountTriggerHandler;
        IDisposable? updateMessagesCountTriggerHandler;
        bool sidebarExpanded = true;

        async Task OpenNotificationsPopup() =>
            await DialogService.OpenAsync<NotificationsPopup>($"Уведомления", null, new DialogOptions() { Width = "750px", Height = "450px" });

        async Task OpenMessagesPopup() =>
            await DialogService.OpenAsync<MessagesPopup>($"Сообщения", null, new DialogOptions() { Width = "750px", Height = "450px" });

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (CurrentState.Account != null && updateNotificationsCountTriggerHandler == null)
            {
                updateNotificationsCountTriggerHandler = updateNotificationsCountTriggerHandler.SignalRClient<UpdateNotificationsCountModel>(CurrentState, async (response) =>
                {
                    var apiResponseNotifCount = await _repoNotifCount.HttpPostAsync(new GetNotificationsCountModel { Token = CurrentState.Account.Token });
                    var notificationsCount = apiResponseNotifCount.Response;
                    await _JSProcessor.ChangeNumberInButtonsFadeInOut("sw-header-UnreadNotificationsNumber", notificationsCount.UnreadCount);
                });
                await CurrentState.SignalRServerAsync(EnumSignalRHandlers.UpdateNotificationsCountServer, CurrentState.Account.Id);

                updateMessagesCountTriggerHandler = updateMessagesCountTriggerHandler.SignalRClient<UpdateMessagesCountModel>(CurrentState, async (response) =>
                {
                    var apiResponseMessagesCount = await _repoCount.HttpPostAsync(new GetMessagesCountModel { Token = CurrentState.Account.Token });
                    var messagesCount = apiResponseMessagesCount.Response;
                    await _JSProcessor.ChangeNumberInButtonsFadeInOut("sw-header-UnreadMessagesNumber", messagesCount.UnreadCount);
                });
                await CurrentState.SignalRServerAsync(EnumSignalRHandlers.UpdateMessagesCountServer, CurrentState.Account.Id);
            }
        }

        void ShowTooltip(ElementReference elementReference, TooltipOptions? options = null) =>
            tooltipService.Open(elementReference, options?.Text, options);

        async void MenuClickAsync(RadzenProfileMenuItem item)
        {
            if (item.Icon == "logout")
                await CurrentState.LogOutAsync();
        }
    }
}
