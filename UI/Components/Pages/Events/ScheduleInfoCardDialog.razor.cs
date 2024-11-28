using Common.Dto.Requests;
using Common.Models.SignalR;
using Common.Models.States;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Events
{
    public partial class ScheduleInfoCardDialog : InfoBase, IDisposable
    {
        [Parameter, EditorRequired] public int ScheduleId { get; set; }

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
    }
}
