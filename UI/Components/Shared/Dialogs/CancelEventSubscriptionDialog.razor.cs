using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
{
    public partial class CancelEventSubscriptionDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        [Inject] IRepository<EventRegistrationRequestDto, EventRegistrationResponseDto> _repoUpdateRegistration { get; set; } = null!;

        async Task Submit()
        {
            if (CurrentState.Account != null)
            {
                MudDialog.Close(DialogResult.Ok(true));

                var apiResponse = await _repoUpdateRegistration.HttpPostAsync(new EventRegistrationRequestDto
                {
                    Token = CurrentState.Account.Token,
                    ScheduleId = ScheduleForEventView.Id
                });

                await CurrentState.ReloadAccountAsync();

                var request = new SignalGlobalRequest
                {
                    OnScheduleChanged = new OnScheduleChanged { EventId = ScheduleForEventView.EventId, ScheduleId = ScheduleForEventView.Id }
                };
                await CurrentState.SignalRServerAsync(request);
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}
