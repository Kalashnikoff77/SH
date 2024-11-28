using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Dialogs;

namespace UI.Components.Pages.Events
{
    public partial class ScheduleInfoCardDialog : IDisposable
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public int ScheduleId { get; set; }

        [Inject] IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> _repoGetSchedules { get; set; } = null!;
        [Inject] IRepository<GetSchedulesDatesRequestDto, GetSchedulesDatesResponseDto> _repoGetSchedulesDates { get; set; } = null!;
        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        SchedulesDatesViewDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesDatesViewDto> scheduleDates { get; set; } = null!;

        IDisposable? OnScheduleChangedHandler;

        protected override async Task OnInitializedAsync()
        {
            ScheduleForEventView = await GetScheduleForEvent(ScheduleId);
            scheduleDates = await GetScheduleDates(ScheduleForEventView.EventId);
            selectedSchedule = scheduleDates.First(s => s.Id == ScheduleForEventView.Id);   // Установим текущую дату мероприятия в выпадающем меню дат
        }

        protected override void OnAfterRender(bool firstRender)
        {
            OnScheduleChangedHandler = OnScheduleChangedHandler.SignalRClient<OnScheduleChangedResponse>(CurrentState, async (response) =>
            {
                var apiResponse = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { ScheduleId = response.ScheduleId });
                if (apiResponse.Response.Schedule != null)
                {
                    ScheduleForEventView = apiResponse.Response.Schedule;
                    await InvokeAsync(StateHasChanged);
                }
            });
        }

        async Task ScheduleChangedAsync(SchedulesDatesViewDto schedule)
        {
            ScheduleForEventView = await GetScheduleForEvent(schedule.Id);
            selectedSchedule = scheduleDates.First(s => s.Id == ScheduleForEventView.Id);   // Установим текущую дату мероприятия в выпадающем меню дат
        }

        async Task<SchedulesForEventsViewDto> GetScheduleForEvent(int scheduleId)
        {
            var response = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { ScheduleId = scheduleId });
            if (response.Response.Schedule == null)
                throw new Exception("Не найдено мероприятие!");
            return response.Response.Schedule;
        }

        async Task<IEnumerable<SchedulesDatesViewDto>> GetScheduleDates(int eventId)
        {
            var responseSchedulesDates = await _repoGetSchedulesDates.HttpPostAsync(new GetSchedulesDatesRequestDto { EventId = eventId });
            if (responseSchedulesDates.Response.SchedulesDates == null)
                throw new Exception("Не найдено на одного расписания у мероприятия!");
            return responseSchedulesDates.Response.SchedulesDates;
        }

        public void Dispose() =>
            OnScheduleChangedHandler?.Dispose();
    }
}
