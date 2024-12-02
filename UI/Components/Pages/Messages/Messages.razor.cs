using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Pages.Messages
{
    public partial class Messages : IDisposable
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetLastMessagesListRequestDto, GetLastMessagesListResponseDto> _repoGetLastMessagesList { get; set; } = null!;

        MudDataGrid<LastMessagesListViewDto> dataGrid = null!;
        GetLastMessagesListRequestDto request = new GetLastMessagesListRequestDto();
        List<LastMessagesListViewDto> LastMessagesList = new List<LastMessagesListViewDto>();
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        // Текущая отображаемая страница с мероприятиями
        int currentPage = 0;
        // Текущее кол-во отображаемых мероприятий на странице
        int currentPageSize = 10;


        protected override async Task OnParametersSetAsync()
        {
        }

        async Task<GridData<LastMessagesListViewDto>> ServerReload(GridState<LastMessagesListViewDto> state)
        {
            var items = new GridData<LastMessagesListViewDto>();

            request.Skip = state.Page * state.PageSize;
            request.Take = state.PageSize;
            request.Token = CurrentState.Account!.Token;

            var apiResponse = await _repoGetLastMessagesList.HttpPostAsync(request);
            if (apiResponse.Response.LastMessagesList != null)
            {
                LastMessagesList = apiResponse.Response.LastMessagesList;

                items = new GridData<LastMessagesListViewDto>
                {
                    Items = LastMessagesList,
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


        public void Dispose()
        {
        }
    }
}
