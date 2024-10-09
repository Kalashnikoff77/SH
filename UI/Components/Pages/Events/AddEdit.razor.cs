using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Net;
using UI.Models;

namespace UI.Components.Pages.Events
{
    public class TabPanel
    {
        public Dictionary<string, bool> Items { get; set; } = null!;
    }

    public partial class AddEdit
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<EventCheckAddingRequestDto, EventCheckAddingResponseDto> _repoCheckAdding { get; set; } = null!;
        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvent { get; set; } = null!;
        [Parameter] public int? EventId { get; set; }

        EventsViewDto Event = new EventsViewDto() 
        {
            Name = "Название мероприятия в клубе",
            Description = "Длинное описание, которое должно быть более пятидесяти символов в длину, иначе не прокатит",
            MaxPairs = 10,
            MaxMen = 5,
            MaxWomen = 15
        };

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool processingPhoto = false;
        bool processingEvent = false;

        bool IsPanel1Valid => TabPanels[1].Items.All(x => x.Value == true);
        bool IsPanel2Valid => TabPanels[2].Items.All(x => x.Value == true);
        bool IsPanel3Valid => TabPanels[3].Items.All(x => x.Value == true);

        protected override void OnInitialized()
        {
            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, bool>
                    {
                        { nameof(Event.Name), false },
                        { nameof(Event.Description), false },
                        { nameof(Event.MaxPairs), false },
                        { nameof(Event.MaxMen), false },
                        { nameof(Event.MaxWomen), false }
                    } }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { "Schedule", false } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { "Photo", false } } } }
            };
        }

        protected async override Task OnParametersSetAsync()
        {
            if (EventId != null)
            {
                var apiResponse = await _repoGetEvent.HttpPostAsync(new GetEventsRequestDto { EventId = EventId });
                if (apiResponse.StatusCode == HttpStatusCode.OK && apiResponse.Response.Event != null)
                {
                    Event = apiResponse.Response.Event;
                }
                else
                    EventId = null;
            }
        }


        #region /// 1. ОБЩЕЕ ///
        Color NameIconColor = Color.Default;
        async Task<string?> NameValidator(string? text)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(text) || text.Length < StaticData.DB_EVENT_NAME_MIN)
            {
                errorMessage = $"Введите не менее {StaticData.DB_EVENT_NAME_MIN} символов";
            }
            else
            {
                var apiResponse = await _repoCheckAdding.HttpPostAsync(new EventCheckAddingRequestDto { EventName = text, Token = CurrentState.Account!.Token });
                if (apiResponse.Response.EventNameExists)
                    errorMessage = $"Это имя уже занято. Выберите другое.";
            }

            CheckPanel1Properties(errorMessage, nameof(Event.Name), ref NameIconColor);
            return errorMessage;
        }

        Color DescriptionIconColor = Color.Default;
        string? DescriptionValidator(string? text)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(text) || text.Length < StaticData.DB_EVENT_DESCRIPTION_MIN)
                errorMessage = $"Введите не менее {StaticData.DB_EVENT_DESCRIPTION_MIN} символов";

            CheckPanel1Properties(errorMessage, nameof(Event.Description), ref DescriptionIconColor);
            return errorMessage;
        }

        Color MaxPairsIconColor = Color.Default;
        string? MaxPairsValidator(short? num)
        {
            string? errorMessage = null;
            if (!num.HasValue)
                errorMessage = "Укажите значение от 0 до 500";
            if (num < 0 || num > 500)
                errorMessage = "Кол-во от 1 до 500";

            CheckPanel1Properties(errorMessage, nameof(Event.MaxPairs), ref MaxPairsIconColor);
            return errorMessage;
        }

        Color MaxMenIconColor = Color.Default;
        string? MaxMenValidator(short? num)
        {
            string? errorMessage = null;
            if (!num.HasValue)
                errorMessage = "Укажите значение от 0 до 500";
            if (num < 0 || num > 500)
                errorMessage = "Кол-во от 1 до 500";

            CheckPanel1Properties(errorMessage, nameof(Event.MaxMen), ref MaxMenIconColor);
            return errorMessage;
        }

        Color MaxWomenIconColor = Color.Default;
        string? MaxWomenValidator(short? num)
        {
            string? errorMessage = null;
            if (!num.HasValue)
                errorMessage = "Укажите значение от 0 до 500";
            if (num < 0 || num > 500)
                errorMessage = "Кол-во от 1 до 500";

            CheckPanel1Properties(errorMessage, nameof(Event.MaxWomen), ref MaxWomenIconColor);
            return errorMessage;
        }

        void CheckPanel1Properties(string? errorMessage, string property, ref Color iconColor)
        {
            if (errorMessage == null)
            {
                TabPanels[1].Items[property].IsValid = true;
                iconColor = Color.Success;
            }
            else
            {
                TabPanels[1].Items[property].IsValid = false;
                iconColor = Color.Error;
            }
            StateHasChanged();
        }

        async Task AddPublicScheduleDialogAsync(MouseEventArgs args)
        {

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
