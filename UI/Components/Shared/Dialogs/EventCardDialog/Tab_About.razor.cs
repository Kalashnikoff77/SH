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
        [Parameter, EditorRequired] public EventCallback<int> ScheduleChangedAsyncCallback { get; set; }

        SchedulesForEventsDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesForEventsDto> schedules { get; set; } = null!;

        protected override void OnParametersSet()
        {
            if (ScheduleForEventView.Event?.Schedule != null)
            {
                schedules = ScheduleForEventView.Event.Schedule.Select(s => s);     // Получим массив расписания по событию
                selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкертное расписание передаваемой встречи
            }
        }

        async Task OnScheduleChangedAsync(SchedulesForEventsDto schedule) 
        {
            selectedSchedule = schedules.FirstOrDefault(s => s.Id == schedule.Id) ?? selectedSchedule;
            await ScheduleChangedAsyncCallback.InvokeAsync(schedule.Id);
        }


        // Тестирую календарик с этим методом
        //async Task OnScheduleDateChangedAsync(DateTime? date)
        //{
        //    var sch = ScheduleForEventView.Event.Schedule.Select(s => s).Where(w => w.StartDate.Date == date).First();
        //    var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = sch.Id });
        //    ScheduleForEventView = eventResponse.Response.Event;
        //}
    }
}
