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
        [Inject] IRepository<GetRegionsForEventsRequestDto, GetRegionsForEventsResponseDto> _repoGetRegions { get; set; } = null!;
        [Inject] IRepository<GetAdminsForEventsRequestDto, GetAdminsForEventsResponseDto> _repoGetAdmins { get; set; } = null!;
        [Inject] IDialogService Dialog { get; set; } = null!;

        MudDataGrid<SchedulesForEventsViewDto> dataGrid = null!;

        GetEventsRequestDto request = new GetEventsRequestDto { IsPhotosIncluded = true };

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
            var apiResponse = await _repoGetEvents.HttpPostAsync(request);
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
            request.FilterFreeText = text;
            return dataGrid.ReloadServerData();
        }

        Task OnFeaturesSelected(IEnumerable<FeaturesDto> selectedItems)
        {
            request.FeaturesIds = selectedItems.Select(s => s.Id);
            return dataGrid.ReloadServerData();
        }

        Task OnCountriesSelected(IEnumerable<RegionsForEventsViewDto> selectedItems)
        {
            request.RegionsIds = selectedItems.Select(s => s.Id);
            return dataGrid.ReloadServerData();
        }

        Task OnAdminsSelected(IEnumerable<AccountsDto> selectedItems)
        {
            request.AdminsIds = selectedItems.Select(s => s.Id);
            return dataGrid.ReloadServerData();
        }


        Task ShowEventCardAsync(SchedulesForEventsViewDto Event)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<Dialogs.EventCardDialog.EventCardDialog>
            {
                { x => x.ScheduleForEvent, Event }
            };
            return Dialog.ShowAsync<Dialogs.EventCardDialog.EventCardDialog>(Event.Event?.Name, dialogParams, dialogOptions);
        }

    }
}
