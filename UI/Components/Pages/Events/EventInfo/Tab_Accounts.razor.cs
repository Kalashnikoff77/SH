using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using UI.Components.Dialogs;

namespace UI.Components.Pages.Events.EventInfo
{
    public partial class Tab_Accounts
    {
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;

        [Inject] IRepository<GetSchedulesForAccountsRequestDto, GetSchedulesForAccountsResponseDto> _repoGetSchedulesForAccounts { get; set; } = null!;

        IEnumerable<SchedulesForAccountsViewDto> registeredAccounts { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {
            var response = await _repoGetSchedulesForAccounts.HttpPostAsync(new GetSchedulesForAccountsRequestDto { ScheduleId = ScheduleForEventView.Id });
            registeredAccounts = response.Response.Accounts;
        }
    }
}
