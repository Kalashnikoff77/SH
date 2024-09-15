﻿using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Shared.Dialogs.EventCardDialog;

namespace UI.Components.Pages
{
    public partial class Events
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;
        [Inject] IRepository<GetFeaturesForEventsRequestDto, GetFeaturesForEventsResponseDto> _repoGetFeatures { get; set; } = null!;
        [Inject] IRepository<GetRegionsForEventsRequestDto, GetRegionsForEventsResponseDto> _repoGetRegions { get; set; } = null!;
        [Inject] IRepository<GetAdminsForEventsRequestDto, GetAdminsForEventsResponseDto> _repoGetAdmins { get; set; } = null!;
        [Inject] IDialogService Dialog { get; set; } = null!;

        MudDataGrid<SchedulesForEventsViewDto> dataGrid = null!;

        GetEventsRequestDto request = new GetEventsRequestDto { IsPhotosIncluded = true };

        List<SchedulesForEventsViewDto> EventsList = new List<SchedulesForEventsViewDto>();

        List<FeaturesForEventsViewDto> FeaturesList = new List<FeaturesForEventsViewDto>();
        List<RegionsForEventsViewDto> RegionsList = new List<RegionsForEventsViewDto>();
        List<AdminsForEventsViewDto> AdminsList = new List<AdminsForEventsViewDto>();

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        protected override async Task OnInitializedAsync()
        {
            var eventsResponse = await _repoGetEvents.HttpPostAsync(new GetEventsRequestDto() { IsPhotosIncluded = true });
            EventsList = eventsResponse.Response.Events;

            var featuresResponse = await _repoGetFeatures.HttpPostAsync(new GetFeaturesForEventsRequestDto());
            FeaturesList = featuresResponse.Response.FeaturesForEvents;

            var regionsResponse = await _repoGetRegions.HttpPostAsync(new GetRegionsForEventsRequestDto());
            RegionsList = regionsResponse.Response.RegionsForEvents;

            var adminsResponse = await _repoGetAdmins.HttpPostAsync(new GetAdminsForEventsRequestDto());
            AdminsList = adminsResponse.Response.AdminsForEvents;
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


        Task ShowEventCardAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<EventCardDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return Dialog.ShowAsync<EventCardDialog>(schedule.Event?.Name, dialogParams, dialogOptions);
        }

    }
}
