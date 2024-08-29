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

        void Close() => MudDialog.Close(DialogResult.Ok(true));

        async Task ScheduleChangedAsync(int scheduleId)
        {
            var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = scheduleId });
            ScheduleForEventView = eventResponse.Response.Event;
        }
    }
}
