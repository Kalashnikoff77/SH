using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.JSProcessor;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared.Dialogs.EventCardDialog
{
    public partial class Tab_Messages
    {
        [Inject] CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        [Inject] IRepository<GetDiscussionsForEventsRequestDto, GetDiscussionsForEventsResponseDto> _repoGetDiscussions { get; set; } = null!;
        [Inject] IRepository<AddDiscussionsForEventsRequestDto, AddDiscussionsForEventsResponseDto> _repoAddDiscussion { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        List<DiscussionsForEventsViewDto> discussions = new List<DiscussionsForEventsViewDto>();
        string? _message { get; set; } = null!;
        bool _sending;
        int _currentElement = 0;
        bool moreDiscussionsButton = false;

        protected override async Task OnInitializedAsync() =>
            await GetDiscussionsAsync();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                await _JSProcessor.ScrollToElement("ChatMessageFrame", $"id_{_currentElement}");
        }

        async Task GetDiscussionsAsync()
        {
            var response = await _repoGetDiscussions.HttpPostAsync(new GetDiscussionsForEventsRequestDto()
            {
                EventId = ScheduleForEventView.EventId,
                GetPreviousFromId = discussions.Count > 0 ? discussions.Min(m => m.Id) : null,
                Take = StaticData.EVENT_DISCUSSIONS_PER_BLOCK
            });
            _currentElement = discussions.Any() ? discussions.Min(m => m.Id) : 0;
            discussions.InsertRange(0, response.Response.Discussions);

            moreDiscussionsButton = discussions.Count < response.Response.NumOfDiscussions;
        }


        async Task OnMessageAdded()
        {
            if (!string.IsNullOrWhiteSpace(_message))
            {
                _sending = true;
                var response = await _repoAddDiscussion.HttpPostAsync(new AddDiscussionsForEventsRequestDto()
                {
                    Token = CurrentState.Account?.Token,
                    EventId = ScheduleForEventView.EventId,
                    Text = _message
                });

                _message = null;
                await GetDiscussionsAsync();
                _sending = false;
            }
        }

        //async ValueTask<ItemsProviderResult<DiscussionsForEventsViewDto>> LoadDiscussions(ItemsProviderRequest request)
        //{
        //    var response = await _repoGetDiscussions.HttpPostAsync(new GetDiscussionsForEventsRequestDto()
        //    {
        //        EventId = ScheduleForEventView.EventId,
        //        Skip = request.StartIndex,
        //        Take = request.Count
        //    });
        //    discussions = response.Response.Discussions;

        //    return new ItemsProviderResult<DiscussionsForEventsViewDto>(discussions, response.Response.NumOfDiscussions);
        //}
    }
}
