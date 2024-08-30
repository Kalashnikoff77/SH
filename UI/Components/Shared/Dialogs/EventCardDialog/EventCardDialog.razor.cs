using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class EventCardDialog
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        [Inject] IRepository<GetEventOneRequestDto, GetEventOneResponseDto> _repoGetEvent { get; set; } = null!;

        MudCarousel<PhotosForEventsDto> Carousel = null!;
        SchedulesForEventsDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesForEventsDto> schedules { get; set; } = null!;

        protected override void OnInitialized()
        {
            if (ScheduleForEventView.Event?.Schedule != null)
            {
                schedules = ScheduleForEventView.Event.Schedule.Select(s => s);     // Получим массив расписания по событию
                selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкретное расписание передаваемой встречи
            }
        }


        async Task ScheduleChangedAsync(SchedulesForEventsDto schedule)
        {
            var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = schedule.Id });
            ScheduleForEventView = eventResponse.Response.Event;
            selectedSchedule = schedule;
        }
    }
}
