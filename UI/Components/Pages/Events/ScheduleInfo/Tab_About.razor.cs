using Common.Dto.Views;
using Common.JSProcessor;
using Microsoft.AspNetCore.Components;
using UI.Components.Dialogs;

namespace UI.Components.Pages.Events.ScheduleInfo
{
    public partial class Tab_About
    {
        [Parameter, EditorRequired] public SchedulesForEventsViewDto ScheduleForEventView { get; set; } = null!;
        [Inject] ShowDialogs ShowDialogs { get; set; } = null!;
        [Inject] IJSProcessor _JSProcessor { get; set; } = null!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await _JSProcessor.ScrollToElement("top");
        }
    }
}
