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
        [Parameter, EditorRequired] public int ScheduleId { get; set; }

        [Inject] IRepository<GetEventOneRequestDto, GetEventOneResponseDto> _repoGetEvent { get; set; } = null!;
        SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        protected override async Task OnParametersSetAsync()
        {
            var eventResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto() { ScheduleId = ScheduleId });
            ScheduleForEventView = eventResponse.Response.Event;
        }

        void Submit() => MudDialog.Close(DialogResult.Ok(true));

        void ScheduleIdChanged(int scheduleId)
        {
            ScheduleId = scheduleId;
        }
    }
}
