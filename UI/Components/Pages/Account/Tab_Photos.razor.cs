using Common.Dto.Views;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Account
{
    public partial class Tab_Photos
    {
        [Parameter] public AccountsViewDto Account { get; set; } = null!;
    }
}
