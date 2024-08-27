using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;
using UI.Components.Dialogs;
using static MudBlazor.CategoryTypes;

namespace UI.Components.Pages
{
    public partial class Events
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;
        [Inject] IRepository<GetFeaturesRequestDto, GetFeaturesResponseDto> _repoGetFeatures { get; set; } = null!;
        [Inject] IRepository<GetRegionsForEventsRequestDto, GetRegionsForEventsResponseDto> _repoGetRegions { get; set; } = null!;
        [Inject] IRepository<GetAdminsForEventsRequestDto, GetAdminsForEventsResponseDto> _repoGetAdmins { get; set; } = null!;
        [Inject] IDialogService Dialog { get; set; } = null!;

        MudDataGrid<SchedulesForEventsViewDto> dataGrid = null!;
        string? filterValue = null;
        List<int> featuresIds = new List<int>();
        List<int> regionsIds = new List<int>();
        List<int> adminsIds = new List<int>();

        List<SchedulesForEventsViewDto> EventsList = new List<SchedulesForEventsViewDto>();

        List<FeaturesDto> FeaturesList = new List<FeaturesDto>();
        List<RegionsForEventsViewDto> RegionsList = new List<RegionsForEventsViewDto>();
        List<AccountsDto> AdminsList = new List<AccountsDto>();

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        protected override async Task OnInitializedAsync()
        {
            var eventsResponse = await _repoGetEvents.HttpPostAsync(new GetEventsRequestDto() { IsPhotosIncluded = true });
            EventsList = eventsResponse.Response.Events;

            var featuresResponse = await _repoGetFeatures.HttpPostAsync(new GetFeaturesRequestDto());
            FeaturesList = featuresResponse.Response.Features;

            var regionsResponse = await _repoGetRegions.HttpPostAsync(new GetRegionsForEventsRequestDto());
            RegionsList = regionsResponse.Response.RegionsForEvents;

            var adminsResponse = await _repoGetAdmins.HttpPostAsync(new GetAdminsForEventsRequestDto());
            if (adminsResponse != null && adminsResponse.Response.Admins?.Count() > 0)
                AdminsList = adminsResponse.Response.Admins;
        }


        async Task<GridData<SchedulesForEventsViewDto>> ServerReload(GridState<SchedulesForEventsViewDto> state)
        {
            var apiResponse = await _repoGetEvents.HttpPostAsync(new GetEventsRequestDto
            {
                FilterFreeText = filterValue,
                FeaturesIds = featuresIds,
                RegionsIds = regionsIds,
                AdminsIds = adminsIds,
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

        Task OnFeaturesSelected(IEnumerable<string> selectedItems)
        {
            featuresIds.Clear();
            foreach (var selectedItem in selectedItems)
            {
                var item = FeaturesList.Where(w => w.Name == selectedItem).FirstOrDefault();
                if (item != null)
                    featuresIds.Add(item.Id);
            }
            return dataGrid.ReloadServerData();
        }

        Task OnCountriesSelected(IEnumerable<string> selectedItems)
        {
            regionsIds.Clear();
            foreach (var selectedItem in selectedItems)
            {
                var item = RegionsList.Where(w => w.Name == selectedItem).FirstOrDefault();
                if (item != null)
                    regionsIds.Add(item.Id);
            }
            return dataGrid.ReloadServerData();
        }

        Task OnAdminsSelected(IEnumerable<string> selectedItems)
        {
            adminsIds.Clear();
            foreach (var selectedItem in selectedItems)
            {
                var item = AdminsList.Where(w => w.Name == selectedItem).FirstOrDefault();
                if (item != null)
                    adminsIds.Add(item.Id);
            }
            return dataGrid.ReloadServerData();
        }


        Task ShowEventCardAsync(EventsViewDto Event)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<EventCardDialog>
            {
                { x => x.Event, Event }
            };
            return Dialog.ShowAsync<EventCardDialog>(Event.Name, dialogParams, dialogOptions);
        }

    }
}
