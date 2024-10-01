using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Shared.Dialogs;

namespace UI.Components.Pages
{
    public partial class Events : IDisposable
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> _repoGetSchedules { get; set; } = null!;
        [Inject] IRepository<GetFeaturesForEventsRequestDto, GetFeaturesForEventsResponseDto> _repoGetFeatures { get; set; } = null!;
        [Inject] IRepository<GetRegionsForEventsRequestDto, GetRegionsForEventsResponseDto> _repoGetRegions { get; set; } = null!;
        [Inject] IRepository<GetAdminsForEventsRequestDto, GetAdminsForEventsResponseDto> _repoGetAdmins { get; set; } = null!;

        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;

        MudDataGrid<SchedulesForEventsViewDto> dataGrid = null!;
        GetSchedulesRequestDto request = new GetSchedulesRequestDto { IsPhotosIncluded = true };

        List<SchedulesForEventsViewDto> EventsList = new List<SchedulesForEventsViewDto>();

        List<FeaturesForEventsViewDto> FeaturesList = new List<FeaturesForEventsViewDto>();
        List<RegionsForEventsViewDto> RegionsList = new List<RegionsForEventsViewDto>();
        List<AdminsForEventsViewDto> AdminsList = new List<AdminsForEventsViewDto>();

        MudCarousel<PhotosForEventsDto> Carousel = null!;

        IDisposable? OnScheduleChangedHandler;

        protected override async Task OnInitializedAsync()
        {
            var featuresResponse = await _repoGetFeatures.HttpPostAsync(new GetFeaturesForEventsRequestDto());
            FeaturesList = featuresResponse.Response.FeaturesForEvents;

            var regionsResponse = await _repoGetRegions.HttpPostAsync(new GetRegionsForEventsRequestDto());
            RegionsList = regionsResponse.Response.RegionsForEvents;

            var adminsResponse = await _repoGetAdmins.HttpPostAsync(new GetAdminsForEventsRequestDto());
            AdminsList = adminsResponse.Response.AdminsForEvents;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            OnScheduleChangedHandler = OnScheduleChangedHandler.SignalRClient(CurrentState, (Func<OnScheduleChangedResponse, Task>)(async (response) =>
            {
                var apiResponse = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { EventId = response.EventId });
                if (apiResponse.Response.Schedules != null)
                {
                    int index;
                    foreach (var sch in apiResponse.Response.Schedules)
                    {
                        index = EventsList.FindIndex(s => s.Id == sch.Id);
                        if (index >= 0) // Есть ли в области видимости браузера такое расписание?
                            EventsList[index] = sch;
                    }
                    await InvokeAsync(StateHasChanged);
                }
            }));
        }


        async Task<GridData<SchedulesForEventsViewDto>> ServerReload(GridState<SchedulesForEventsViewDto> state)
        {
            var items = new GridData<SchedulesForEventsViewDto>();

            var apiResponse = await _repoGetSchedules.HttpPostAsync(request);
            if (apiResponse.Response.Schedules != null)
            {
                EventsList = apiResponse.Response.Schedules;

                items = new GridData<SchedulesForEventsViewDto>
                {
                    Items = EventsList,
                    TotalItems = EventsList.Count
                };
            }
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

        public void Dispose() =>
            OnScheduleChangedHandler?.Dispose();
    }
}
