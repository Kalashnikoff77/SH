using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages
{
    public partial class Events
    {
        [Inject] IRepository<GetEventsModel, GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;

        List<EventsViewDto> EventsList { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetEvents .HttpPostAsync(new GetEventsModel() { IsPhotosIncluded = true });
            EventsList = apiResponse.Response.Events;
        }


    }
}
