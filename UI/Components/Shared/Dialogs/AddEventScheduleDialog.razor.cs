using Common.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
{
    public partial class AddEventScheduleDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

        bool IsFormValid = false;

        SchedulesForEventsDto Schedule { get; set; } = new SchedulesForEventsDto();

        void Submit() => MudDialog.Close(DialogResult.Ok(Schedule));

        void Cancel() => MudDialog.Cancel();
    }
}
