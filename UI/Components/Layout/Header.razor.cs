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

namespace UI.Components.Layout
{
    public partial class Header
    {
        [Inject] IRepository<GetNotificationsCountModel, GetNotificationsCountRequestDto, GetNotificationsCountResponseDto> _repoNotifCount { get; set; } = null!;
        [Inject] IRepository<GetNotificationsModel, GetNotificationsRequestDto, GetNotificationsResponseDto> _repoNotifications { get; set; } = null!;
        [Inject] IRepository<GetMessagesCountModel, GetMessagesCountRequestDto, GetMessagesCountResponseDto> _repoCount { get; set; } = null!;
        [Inject] IRepository<GetLastMessagesListModel, GetLastMessagesListRequestDto, GetLastMessagesListResponseDto> _repoMessages { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        IDisposable? updateNotificationsCountTriggerHandler;
        IDisposable? updateMessagesCountTriggerHandler;
        bool sidebarExpanded = true;
        GetMessagesCountResponseDto? messagesCount;
        GetNotificationsCountResponseDto? notificationsCount;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (CurrentState.Account != null)
            {
                updateNotificationsCountTriggerHandler = updateNotificationsCountTriggerHandler.SignalRClient<UpdateNotificationsCountModel>(CurrentState, async (response) =>
                {
                    var apiResponseCount = await _repoNotifCount.HttpPostAsync(new GetNotificationsCountModel { Token = CurrentState.Account.Token });
                    notificationsCount = apiResponseCount.Response;
                    await _JSProcessor.ChangeNumberAndColorFadeInOut("sw-header-UnreadNotificationsNumber", notificationsCount.UnreadCount);
                });
                await CurrentState.SignalRServerAsync(EnumSignalRHandlers.UpdateNotificationsCountServer, CurrentState.Account.Id);

                updateMessagesCountTriggerHandler = updateMessagesCountTriggerHandler.SignalRClient<UpdateMessagesCountModel>(CurrentState, async (response) =>
                {
                    var apiResponseCount = await _repoCount.HttpPostAsync(new GetMessagesCountModel { Token = CurrentState.Account.Token });
                    messagesCount = apiResponseCount.Response;
                    await _JSProcessor.ChangeNumberAndColorFadeInOut("sw-header-UnreadMessagesNumber", messagesCount.UnreadCount);
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
