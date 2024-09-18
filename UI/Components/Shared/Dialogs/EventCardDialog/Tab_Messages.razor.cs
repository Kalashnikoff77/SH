using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_Messages
    {
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        [Inject] IRepository<GetDiscussionsForEventsRequestDto, GetDiscussionsForEventsResponseDto> _repoGetDiscussions { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        List<DiscussionsForEventsViewDto> discussions = null!;

        protected override async Task OnParametersSetAsync()
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
        }
    }
}
