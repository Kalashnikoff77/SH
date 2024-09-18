using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using MudBlazor;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_Messages
    {
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        [Inject] IRepository<GetDiscussionsForEventsRequestDto, GetDiscussionsForEventsResponseDto> _repoGetDiscussions { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;
        //[Inject] IScrollManager ScrollManager { get; set; } = null!;

        List<DiscussionsForEventsViewDto> discussions = null!;

        protected override async Task OnInitializedAsync()
        {
            var response = await _repoGetDiscussions.HttpPostAsync(new GetDiscussionsForEventsRequestDto()
            {
                EventId = ScheduleForEventView.EventId
            });
            discussions = response.Response.Discussions;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                await _JSProcessor.ScrollDivToBottom("ChatMessageFrame");
                //await ScrollManager.ScrollToBottomAsync("ChatMessageFrame", ScrollBehavior.Auto);
        }


        private async ValueTask<ItemsProviderResult<DiscussionsForEventsViewDto>> LoadDiscussions(ItemsProviderRequest request)
        {
            var response = await _repoGetDiscussions.HttpPostAsync(new GetDiscussionsForEventsRequestDto()
            {
                EventId = ScheduleForEventView.EventId,
                Skip = request.StartIndex,
                Take = request.Count
            });
            discussions = response.Response.Discussions;

            return new ItemsProviderResult<DiscussionsForEventsViewDto>(discussions, response.Response.NumOfDiscussions);
        }
    }
}
