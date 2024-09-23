using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class EventCardDialog : IDisposable
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        [Inject] IRepository<GetScheduleOneRequestDto, GetScheduleOneResponseDto> _repoGetEvent { get; set; } = null!;

        MudCarousel<PhotosForEventsDto> Carousel = null!;
        SchedulesForEventsDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesForEventsDto> schedules { get; set; } = null!;

        IDisposable? OnEventDiscussionAddedHandler;

        protected override void OnInitialized()
        {
            if (ScheduleForEventView.Event?.Schedule != null)
            {
                schedules = ScheduleForEventView.Event.Schedule.Select(s => s);     // Получим массив расписания по событию
                selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкретное расписание передаваемой встречи
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            OnEventDiscussionAddedHandler = OnEventDiscussionAddedHandler.SignalRClient<OnScheduleChangedResponse>(CurrentState, async (response) =>
            {
                ScheduleForEventView = response.ScheduleForEventViewDto;
                await InvokeAsync(StateHasChanged);
            });
        }

        async Task ScheduleChangedAsync(SchedulesForEventsDto schedule)
        {
            var eventResponse = await _repoGetEvent.HttpPostAsync(new GetScheduleOneRequestDto() { ScheduleId = schedule.Id });
            ScheduleForEventView = eventResponse.Response.Event;
            selectedSchedule = schedule;
        }

        public void Dispose() =>
            OnEventDiscussionAddedHandler?.Dispose();
    }
}
