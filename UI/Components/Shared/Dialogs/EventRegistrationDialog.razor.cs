using Common.Dto.Views;
using Common.Models.States;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
{
    public partial class EventRegistrationDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        void Submit() => MudDialog.Close(DialogResult.Ok(true));

        void Cancel() => MudDialog.Cancel();
    }
}
