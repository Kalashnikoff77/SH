using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Messages
{
    public partial class Messages : IDisposable
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<GetLastMessagesListRequestDto, GetLastMessagesListResponseDto> _repoGetLastMessagesList { get; set; } = null!;

        List<LastMessagesListViewDto>? lastMessagesList { get; set; } = null;

        protected override async Task OnParametersSetAsync()
        {
            if (CurrentState.IsAccountLoggedIn)
            {
                var response = await _repoGetLastMessagesList.HttpPostAsync(new GetLastMessagesListRequestDto { Token = CurrentState.Account!.Token });
                lastMessagesList = response.Response.LastMessagesList;
            }

        }

        public void Dispose()
        {
        }
    }
}
