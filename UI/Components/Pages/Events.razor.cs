using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Pages
{
    public partial class Events
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;
        [Inject] IRepository<GetFeaturesRequestDto, GetFeaturesResponseDto> _repoGetFeatures { get; set; } = null!;

        MudDataGrid<SchedulesForEventsViewDto> dataGrid = null!;
        string? filterValue = null;
        List<int> featuresIds = new List<int>();

        List<SchedulesForEventsViewDto> EventsList = new List<SchedulesForEventsViewDto>();
        List<FeaturesDto> FeaturesList = new List<FeaturesDto>();

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        protected override async Task OnInitializedAsync()
        {
            var eventsResponse = await _repoGetEvents.HttpPostAsync(new GetEventsRequestDto() { IsPhotosIncluded = true });
            EventsList = eventsResponse.Response.Events;

            var featuresResponse = await _repoGetFeatures.HttpPostAsync(new GetFeaturesRequestDto());
            FeaturesList = featuresResponse.Response.Features;
        }


        async Task<GridData<SchedulesForEventsViewDto>> ServerReload(GridState<SchedulesForEventsViewDto> state)
        {
            var apiResponse = await _repoGetEvents.HttpPostAsync(new GetEventsRequestDto
            {
                FilterFreeText = filterValue,
                FeaturesIds = featuresIds,
                IsPhotosIncluded = true
            });
            EventsList = apiResponse.Response.Events;

            var items = new GridData<SchedulesForEventsViewDto>
            {
                Items = EventsList.ToArray(),
                TotalItems = EventsList.Count
            };
            return items;
        }

        Task OnSearch(string text)
        {
            filterValue = text;
            return dataGrid.ReloadServerData();
        }

        Task OnFeaturesSelected(IEnumerable<string> selectedFeatures)
        {
            featuresIds.Clear();

            foreach (var feat in selectedFeatures)
            {
                var feature = FeaturesList.Where(w => w.Name == feat).FirstOrDefault();
                if (feature != null)
                    featuresIds.Add(feature.Id);
            }
            return dataGrid.ReloadServerData();
        }
    }
}
