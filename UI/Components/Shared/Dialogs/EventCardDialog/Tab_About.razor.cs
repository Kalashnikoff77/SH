using Common.Dto.Views;
using Common.Dto;
using Common.Models.States;
using Microsoft.AspNetCore.Components;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Repository;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_About
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Parameter] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        [Inject] IRepository<GetEventOneRequestDto, GetEventOneResponseDto> _repoGetEvent { get; set; } = null!;

        IEnumerable<SchedulesForEventsDto> schedules { get; set; } = null!;

        SchedulesForEventsDto selectedSchedule { get; set; } = null!;

        protected override void OnInitialized()
        {
            if (ScheduleForEventView.Event?.Schedule != null)
            {
                schedules = ScheduleForEventView.Event.Schedule.Select(s => s);
                selectedSchedule = schedules.First(w => w.Id == ScheduleForEventView.Id);
            }
        }

        async Task OnScheduleChangedAsync(IEnumerable<SchedulesForEventsDto> items)
        {
            var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = items.First().Id });
            ScheduleForEventView = eventResponse.Response.Event;
        }
    }
}
