using Common.Dto.Views;
using Common.Dto;
using Common.Models.States;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Dialogs.EventCardDialog
{
    public partial class EventCardDialog
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter] public SchedulesForEventsViewDto ScheduleForEvent { get; set; } = null!;

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        void Submit() => MudDialog.Close(DialogResult.Ok(true));
    }
}
