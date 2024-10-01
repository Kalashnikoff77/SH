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
    public partial class EventRegistrationDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        [Inject] IRepository<GetAdminsForEventsRequestDto, GetAdminsForEventsResponseDto> _repoGetAdmins { get; set; } = null!;

        async Task Submit()
        {
            MudDialog.Close(DialogResult.Ok(true));

            // var apiResponse = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { EventId = response.EventId });


            var request = new SignalGlobalRequest
            {
                OnScheduleChanged = new OnScheduleChanged { EventId = ScheduleForEventView.EventId, ScheduleId = ScheduleForEventView.Id }
            };
            await CurrentState.SignalRServerAsync(request);
        }

        void Cancel() => MudDialog.Cancel();
    }
}
