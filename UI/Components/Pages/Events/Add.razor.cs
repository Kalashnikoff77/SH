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

        async Task<string?> NameValidator(string? text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < StaticData.DB_EVENT_NAME_MIN)
                return $"Минимальная длина названия {StaticData.DB_EVENT_NAME_MIN}";
            return null;
        }
        
        string? DescriptionValidator(string? text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < StaticData.DB_EVENT_DESCRIPTION_MIN)
                return $"Минимальная длина описания {StaticData.DB_EVENT_DESCRIPTION_MIN}";
            return null;
        }

        string? MaxPersonsValidator(short? num)
        {
            if (!num.HasValue)
                return "Укажите значение от 0 до 500";
            if (num < 0 || num > 500)
                return "Кол-во от 1 до 500";
            return null;
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
