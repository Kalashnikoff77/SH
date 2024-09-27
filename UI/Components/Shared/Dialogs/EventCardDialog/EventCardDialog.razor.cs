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

        [Inject] IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> _repoGetSchedules { get; set; } = null!;

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
                var apiResponse = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { ScheduleId = response.ScheduleId });
                if (apiResponse.Response.Schedule != null)
                {
                    ScheduleForEventView = apiResponse.Response.Schedule;
                    await InvokeAsync(StateHasChanged);
                }
            });
        }

        async Task ScheduleChangedAsync(SchedulesForEventsDto schedule)
        {
            var eventResponse = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto() { ScheduleId = schedule.Id });
            if (eventResponse.Response.Schedule != null)
            {
                ScheduleForEventView = eventResponse.Response.Schedule;
                selectedSchedule = schedule;
            }
        }

        public void Dispose() =>
            OnEventDiscussionAddedHandler?.Dispose();
    }
}
