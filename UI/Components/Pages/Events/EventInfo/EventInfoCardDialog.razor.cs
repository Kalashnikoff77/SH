﻿using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Dialogs;

namespace UI.Components.Pages.Events.EventInfo
{
    public partial class EventInfoCardDialog : IDisposable
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto? ScheduleForEventView { get; set; }
        [Parameter, EditorRequired] public int ScheduleId { get; set; }

        [Inject] IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> _repoGetSchedules { get; set; } = null!;
        [Inject] IRepository<GetSchedulesDatesRequestDto, GetSchedulesDatesResponseDto> _repoGetSchedulesDates { get; set; } = null!;
        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;

        MudCarousel<PhotosForEventsDto> Carousel = null!;
        SchedulesDatesViewDto selectedSchedule { get; set; } = null!;
        IEnumerable<SchedulesDatesViewDto>? schedulesDates { get; set; } = null!;

        IDisposable? OnEventDiscussionAddedHandler;

        protected override async Task OnInitializedAsync()
        {
            var response = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { ScheduleId = ScheduleId });
            ScheduleForEventView = response.Response.Schedule;
            if (ScheduleForEventView == null)
                throw new Exception("Не найдено расписание!");

            var responseSchedulesDates = await _repoGetSchedulesDates.HttpPostAsync(new GetSchedulesDatesRequestDto { EventId = ScheduleForEventView.EventId });
            schedulesDates = responseSchedulesDates.Response.SchedulesDates;
            if (schedulesDates == null)
                throw new Exception("Не найдено на одного расписания у мероприятия!");

            //    selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкретное расписание передаваемой встречи

            // TODO REMOVE (OK)
            //if (ScheduleForEventView.Event?.Schedule != null)
            //{
            //    schedules = ScheduleForEventView.Event.Schedule.Select(s => s);     // Получим массив расписания по событию
            //    selectedSchedule = schedules.First(s => s.Id == ScheduleForEventView.Id);   // Из массива получим конкретное расписание передаваемой встречи
            //}
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

        async Task ScheduleChangedAsync(SchedulesDatesViewDto schedule)
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
