using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using Radzen;
using System.Drawing;

namespace UI.Components.Pages
{
    public partial class Messages
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

        void DataGridRowRender(RowRenderEventArgs<LastMessagesListViewDto> args)
        {
            args.Attributes.Add("style", "cursor:default");
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
