using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace UI.Components.Layout.Popup
{
    public partial class MessagesPopup
    {
        [Inject] IRepository<GetLastMessagesListModel, GetLastMessagesListRequestDto, GetLastMessagesListResponseDto> _repoGetLastMessagesList { get; set; } = null!;
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;

        List<LastMessagesListViewDto>? LastMessagesList;
        RadzenDataGrid<LastMessagesListViewDto> messagesGrid = null!;
        int TotalCount;
        bool IsLoading;

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null)
            {
                var apiResponse = await _repoGetLastMessagesList.HttpPostAsync(new GetLastMessagesListModel { Token = CurrentState.Account.Token });
                LastMessagesList = apiResponse.Response.LastMessagesList;
                TotalCount = apiResponse.Response.Count;
            }
        }

        async void LoadData(LoadDataArgs args)
        {
            IsLoading = true;

            var request = new GetLastMessagesListModel
            {
                Token = CurrentState.Account?.Token,
                Top = args.Top,
                Skip = args.Skip
            };

            if (args.Filters.Count() > 0)
            {
                var filter = args.Filters.First();
                request.FilterProperty = filter.Property;
                request.FilterValue = filter.FilterValue;
            }

            var apiResponse = await _repoGetLastMessagesList.HttpPostAsync(request);
            LastMessagesList = apiResponse.Response.LastMessagesList;
            TotalCount = apiResponse.Response.Count;

            IsLoading = false;
            StateHasChanged();
        }
    }
}
