using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Layout
{
    public partial class NotificationsWindow
    {
        [Inject] IRepository<GetNotificationsModel, GetNotificationsRequestDto, GetNotificationsResponseDto> _repoGetNotifications { get; set; } = null!;
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Parameter] public UsersDto User { get; set; } = null!;

        List<NotificationsViewDto>? Notifications;

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null)
            {
                var apiResponse = await _repoGetNotifications.HttpPostAsync(new GetNotificationsModel { MarkAsRead = true, Token = CurrentState.Account.Token });
                Notifications = apiResponse.Response.Notifications;
            }
        }

        void OnSubmit(UsersDto User) =>
            dialogService.Close(User);
    }
}
