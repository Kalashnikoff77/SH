using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using UI.Components.Dialogs;

namespace UI.Components.Pages.Messages
{
    public partial class Messages : IDisposable
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetLastMessagesListRequestDto, GetLastMessagesListResponseDto> _repoGetLastMessagesList { get; set; } = null!;
        [Inject] IRepository<MarkMessageAsReadRequestDto, MarkMessageAsReadResponseDto> _markMessageAsRead { get; set; } = null!;
        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;

        List<LastMessagesListViewDto> LastMessagesList = new List<LastMessagesListViewDto>();

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null)
            {
                var request = new GetLastMessagesListRequestDto() { Token = CurrentState.Account.Token };
                var apiResponse = await _repoGetLastMessagesList.HttpPostAsync(request);
                if (apiResponse.Response.LastMessagesList != null)
                {
                    LastMessagesList = apiResponse.Response.LastMessagesList;
                }
            }
        }

        async Task MarkAsReadCallback(int markAsReadId)
        {
            var apiResponse = await _markMessageAsRead.HttpPostAsync(new MarkMessageAsReadRequestDto { MessageId = markAsReadId, Token = CurrentState.Account?.Token });

            var index = LastMessagesList.FindIndex(x => x.Id == markAsReadId);
            if (index >= 0 && apiResponse.Response.UpdatedMessage != null)
                LastMessagesList[index] = apiResponse.Response.UpdatedMessage;
        }

        async Task MarkAllAsReadAsync()
        {
        }

        async Task BlockAccountAsync()
        {
        }

        async Task OnSearch(string text)
        {
        }

        public void Dispose()
        {
        }
    }
}
