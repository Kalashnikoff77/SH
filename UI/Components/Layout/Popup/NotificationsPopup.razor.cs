using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace UI.Components.Layout.Popup
{
    public partial class NotificationsPopup
    {
        [Inject] IRepository<GetNotificationsModel, GetNotificationsRequestDto, GetNotificationsResponseDto> _repoGetNotifications { get; set; } = null!;
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;

        List<NotificationsViewDto>? Notifications;
        RadzenDataGrid<NotificationsViewDto> notificationsGrid = null!;
        int TotalCount;
        bool IsLoading;

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null)
            {
                var apiResponse = await _repoGetNotifications.HttpPostAsync(new GetNotificationsModel { MarkAsRead = true, Token = CurrentState.Account.Token });
                Notifications = apiResponse.Response.Notifications;
                TotalCount = apiResponse.Response.Count;
            }
        }

        async void LoadData(LoadDataArgs args)
        {
            IsLoading = true;

            var request = new GetNotificationsModel 
            {
                Token = CurrentState.Account?.Token,
                MarkAsRead = true,
                Top = args.Top,
                Skip = args.Skip
            };

            if (args.Filters.Count() > 0)
            {
                var filter = args.Filters.First();
                request.FilterProperty = filter.Property;
                request.FilterValue = filter.FilterValue;
            }

            var apiResponse = await _repoGetNotifications.HttpPostAsync(request);
            Notifications = apiResponse.Response.Notifications;
            TotalCount = apiResponse.Response.Count;

            IsLoading = false;
            StateHasChanged();
        }
    }
}
