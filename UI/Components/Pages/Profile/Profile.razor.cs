using Common.Models.States;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Profile
{
    public partial class Profile
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        void UpdateProfile() => StateHasChanged();
    }
}
