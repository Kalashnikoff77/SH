using Common.Dto;
using Common.Dto.Views;
using Common.Models.States;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_About
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        SchedulesForEventsDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesForEventsDto> schedules { get; set; } = null!;

        protected override void OnParametersSet()
        {
            if (ScheduleForEventView.Event?.Schedule != null)
            {
                schedules = ScheduleForEventView.Event.Schedule.Select(s => s);     // Получим массив расписания по событию
                selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкретное расписание передаваемой встречи
            }
        }
    }
}
