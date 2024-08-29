using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_About
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public int ScheduleId { get; set; }
        [Parameter, EditorRequired] public EventCallback<int> ScheduleIdChangedCallback { get; set; }

        [Inject] IRepository<GetEventOneRequestDto, GetEventOneResponseDto> _repoGetEvent { get; set; } = null!;

        SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        SchedulesForEventsDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesForEventsDto> schedules { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = ScheduleId });
            ScheduleForEventView = eventResponse.Response.Event;

            if (ScheduleForEventView.Event?.Schedule != null)
            {
                schedules = ScheduleForEventView.Event.Schedule.Select(s => s);     // Получим массив расписания по событию
                selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкертное расписание передаваемой встречи
            }
        }

        async Task OnScheduleChanged(SchedulesForEventsDto item) 
        {
            selectedSchedule = schedules.FirstOrDefault(s => s.Id == item.Id) ?? selectedSchedule;
            await ScheduleIdChangedCallback.InvokeAsync(item.Id);
        }


        //async Task OnScheduleChangedAsync(IEnumerable<SchedulesForEventsDto> items)
        //{
        //    var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = items.First().Id });
        //    ScheduleForEventView = eventResponse.Response.Event;
        //}

        // Тестирую календарик с этим методом
        //async Task OnScheduleDateChangedAsync(DateTime? date)
        //{
        //    var sch = ScheduleForEventView.Event.Schedule.Select(s => s).Where(w => w.StartDate.Date == date).First();
        //    var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = sch.Id });
        //    ScheduleForEventView = eventResponse.Response.Event;
        //}
    }
}
