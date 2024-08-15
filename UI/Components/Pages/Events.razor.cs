using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Pages
{
    public partial class Events
    {
        [Inject] IRepository<GetEventsModel, GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;

        MudDataGrid<EventsViewDto> dataGrid = null!;
        List<EventsViewDto> EventsList = new List<EventsViewDto>();
        string filterValue = null!;
        
        MudCarousel<EventsPhotosDto> Carousel = null!;

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetEvents.HttpPostAsync(new GetEventsModel() { IsPhotosIncluded = true });
            EventsList = apiResponse.Response.Events;
        }


        private async Task<GridData<EventsViewDto>> ServerReload(GridState<EventsViewDto> state)
        {
            var apiResponse = await _repoGetEvents.HttpPostAsync(new GetEventsModel
            { 
                FilterProperty = nameof(EventsViewDto.Name),
                FilterValue = filterValue,
                IsPhotosIncluded = true
            });
            EventsList = apiResponse.Response.Events;

            var items = new GridData<EventsViewDto>
            {
                Items = EventsList.ToArray(),
                TotalItems = EventsList.Count
            };
            return items;
        }

        private Task OnSearch(string text)
        {
            filterValue = text;
            return dataGrid.ReloadServerData();
        }

    }
}
