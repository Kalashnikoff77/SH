using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Components.Dialogs;

namespace UI.Components.Pages.Events
{
    public partial class Events : IDisposable
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> _repoGetSchedules { get; set; } = null!;
        [Inject] IRepository<GetFeaturesForEventsRequestDto, GetFeaturesForEventsResponseDto> _repoGetFeatures { get; set; } = null!;
        [Inject] IRepository<GetRegionsForEventsRequestDto, GetRegionsForEventsResponseDto> _repoGetRegions { get; set; } = null!;
        [Inject] IRepository<GetAdminsForEventsRequestDto, GetAdminsForEventsResponseDto> _repoGetAdmins { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;

        GetSchedulesRequestDto request = new GetSchedulesRequestDto { IsPhotosIncluded = true };
        List<SchedulesForEventsViewDto> SchedulesList = new List<SchedulesForEventsViewDto>();

        /// <summary>
        /// Для предотвращения повторного выполнения OnParametersSet (выполняется при переходе на другую ссылку)
        /// </summary>
        protected bool isFirstSetParameters = true;

        // Текущая отображаемая страница с мероприятиями
        int currentPage = 0;
        // Текущее кол-во отображаемых мероприятий на странице
        int currentPageSize = 2;

        bool IsButtonMoreVisible = true;

        IDisposable? OnScheduleChangedHandler;

        protected override async Task OnInitializedAsync()
        {
            var featuresResponse = await _repoGetFeatures.HttpPostAsync(new GetFeaturesForEventsRequestDto());
            FeaturesList = featuresResponse.Response.FeaturesForEvents;

            var adminsResponse = await _repoGetAdmins.HttpPostAsync(new GetAdminsForEventsRequestDto());
            AdminsList = adminsResponse.Response.AdminsForEvents;

            var regionsResponse = await _repoGetRegions.HttpPostAsync(new GetRegionsForEventsRequestDto());
            RegionsList = regionsResponse.Response.RegionsForEvents;

            await LoadSchedulesAsync();
        }

        protected override void OnParametersSet()
        {
            // Установим фильтр региона согласно данным учётки пользователя
            if (CurrentState.Account?.Country?.Region != null && isFirstSetParameters)
            {
                //var accountRegion = RegionsList.FirstOrDefault(w => w.Id == CurrentState.Account.Country.Region.Id);
                //if (accountRegion != null)
                //    Filters.SelectedRegions = [accountRegion.Name];

                isFirstSetParameters = false;
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            OnScheduleChangedHandler = OnScheduleChangedHandler.SignalRClient(CurrentState, (Func<OnScheduleChangedResponse, Task>)(async (response) =>
            {
                if (response.UpdatedSchedule != null)
                {
                    // Есть ли в области видимости браузера такое расписание?
                    var index = SchedulesList.FindIndex(i => i.Id == response.UpdatedSchedule.Id);
                    if (index >= 0)
                        SchedulesList[index] = response.UpdatedSchedule;

                    await InvokeAsync(StateHasChanged);
                }
            }));
        }

        async Task MoreSchedulesAsync()
        {
            request.Skip = ++currentPage * currentPageSize;
            await LoadSchedulesAsync(false);
        }

        async Task<GridData<SchedulesForEventsViewDto>> ServerReload(GridState<SchedulesForEventsViewDto> state)
        {
            var items = new GridData<SchedulesForEventsViewDto>();

            request.Skip = state.Page * state.PageSize;
            request.Take = state.PageSize;

            var apiResponse = await _repoGetSchedules.HttpPostAsync(request);
            if (apiResponse.Response.Schedules != null)
            {
                SchedulesList = apiResponse.Response.Schedules;

                items = new GridData<SchedulesForEventsViewDto>
                {
                    Items = SchedulesList,
                    TotalItems = apiResponse.Response.Count ?? 0
                };
            }

            // Проверка, нужно ли прокручивать страницу вверх при переключении на другую страницу
            // или изменении кол-ва мероприятий на странице
            if (state.Page != currentPage || state.PageSize != currentPageSize)
            {
                await _JSProcessor.ScrollToElement("top");
                currentPage = state.Page;
                currentPageSize = state.PageSize;
            }

            return items;
        }

        async Task LoadSchedulesAsync(bool toResetOffset = true)
        {
            if (toResetOffset)
            {
                currentPage = 0;
                request.Skip = currentPage * currentPageSize;
                request.Take = currentPageSize;
            }

            var apiResponse = await _repoGetSchedules.HttpPostAsync(request);
            SchedulesList.AddRange(apiResponse.Response.Schedules ?? new List<SchedulesForEventsViewDto>());
            IsButtonMoreVisible = apiResponse.Response.Count <= SchedulesList.Count ? false : true;
        }

        public void Dispose() =>
            OnScheduleChangedHandler?.Dispose();
    }
}
