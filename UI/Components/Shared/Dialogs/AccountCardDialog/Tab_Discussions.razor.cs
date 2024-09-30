using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared.Dialogs.AccountCardDialog
{
    public partial class Tab_Discussions : IDisposable
    {
        [Parameter, EditorRequired] public AccountsViewDto Account { get; set; } = null!;
        [Inject] IRepository<GetDiscussionsForEventsRequestDto, GetDiscussionsForEventsResponseDto> _repoGetDiscussions { get; set; } = null!;


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
        }


        public void Dispose() { }

    }
}
