using Common.Dto.Requests;
using Common.Dto;
using UI.Models;
using Common.Models;
using MudBlazor;

namespace UI.Components.Pages.Events
{
    public partial class Add
    {
        EventsDto Event = new EventsDto();

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool processingPhoto = false;
        bool processingEvent = false;

        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value.IsValid == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value.IsValid == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value.IsValid == true);

        protected override async Task OnInitializedAsync()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, TabPanelItem> { { "Имя", new TabPanelItem() } } } },
                { 2, new TabPanel { Items = new Dictionary<string, TabPanelItem> { { "Users", new TabPanelItem() } } } },
                { 3, new TabPanel { Items = new Dictionary<string, TabPanelItem> { { "Avatar", new TabPanelItem() } } } }
            };
        }

        #region /// 1. ОБЩЕЕ ///

        Color NameIconColor = Color.Default;
        Color DescriptionIconColor = Color.Default;

        async Task<string?> NameValidator(string name)
        {
            string? errorMessage = null;
            return errorMessage;
        }
        async Task<string?> DescriptionValidator(string name)
        {
            string? errorMessage = null;
            return errorMessage;
        }
        #endregion


        async void SubmitAsync()
        {
            processingEvent = true;
            StateHasChanged();

            processingEvent = false;
            StateHasChanged();
        }

    }
}
