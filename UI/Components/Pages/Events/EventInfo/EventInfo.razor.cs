using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Events.EventInfo
{
    public partial class EventInfo
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public int EventId { get; set; }

        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvent { get; set; } = null!;

        EventsViewDto Event { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var response = await _repoGetEvent.HttpPostAsync(new GetEventsRequestDto { EventId = EventId });
            if (response.Response.Event != null)
                Event = response.Response.Event;
        }
    }
}
