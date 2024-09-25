using Common.Models.States;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Profile
{
    public partial class Photos
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {

        }
    }
}
