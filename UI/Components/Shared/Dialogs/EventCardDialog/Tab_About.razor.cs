using Common.Dto.Views;
using Common.Dto;
using Common.Models.States;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_About
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Parameter] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        SchedulesForEventsDto? selectedSchedule { get; set; }

        protected override void OnInitialized()
        {
            selectedSchedule = ScheduleForEventView;
        }

        Task OnScheduleChangedAsync(IEnumerable<SchedulesForEventsDto> items)
        {
            return Task.CompletedTask;
        }
    }
}
