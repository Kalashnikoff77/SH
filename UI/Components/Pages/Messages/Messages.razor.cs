using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Sp;
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

        List<LastMessagesForAccountSpDto> LastMessagesList = new List<LastMessagesForAccountSpDto>();

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.Account != null)
            {
                var request = new GetLastMessagesListRequestDto() { Token = CurrentState.Account.Token };
                var apiResponse = await _repoGetLastMessagesList.HttpPostAsync(request);
                LastMessagesList = apiResponse.Response.LastMessagesList ?? new List<LastMessagesForAccountSpDto>();
            }
        }

        async Task MarkAsReadCallback(int markAsReadId)
        {
            var index = LastMessagesList.FindIndex(x => x.Id == markAsReadId);
            // Проверим, помечено ли сообщение как прочитанное и адресовано ли нам?
            if (index >= 0 && LastMessagesList[index].Recipient?.Id == CurrentState.Account?.Id && LastMessagesList[index].ReadDate == null)
            {
                var apiResponse = await _markMessageAsRead.HttpPostAsync(new MarkMessageAsReadRequestDto { MessageId = markAsReadId, Token = CurrentState.Account?.Token });
                if (apiResponse.Response.UpdatedMessage != null)
                    LastMessagesList[index] = apiResponse.Response.UpdatedMessage;
            }
        }

        async Task MarkAllAsReadAsync(int markAsReadId)
        {
            var index = LastMessagesList.FindIndex(x => x.Id == markAsReadId);
            if (index >= 0 && LastMessagesList[index].Sender != null)
            {
                var apiResponse = await _markMessageAsRead.HttpPostAsync(new MarkMessageAsReadRequestDto { MessageId = markAsReadId, SenderId = LastMessagesList[index].Sender!.Id, Token = CurrentState.Account?.Token });
                if (apiResponse.Response.UpdatedMessage != null)
                    LastMessagesList[index] = apiResponse.Response.UpdatedMessage;
            }
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
