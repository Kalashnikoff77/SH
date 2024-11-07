using Common.Dto.Requests;
using Common.Dto.Views;
using Common.Models.States;
using UI.Models;

namespace UI.Components.Pages.Events
{
    public partial class AddEvent : EventDtoBase, IDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            // TODO Убрать заполнение тестовыми данными (OK)
            Event = new EventsViewDto()
            {
                Name = "Название мероприятия в клубе",
                Description = "Длинное описание, которое должно быть более пятидесяти символов в длину, иначе не прокатит",
                Address = "МО, пос. Каменка, д. 12",
                MaxPairs = 10,
                MaxMen = 5,
                MaxWomen = 15
            };

            var apiCountriesResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesRequestDto());
            countries = apiCountriesResponse.Response.Countries;

            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, bool>
                    {
                        { nameof(Event.Name), false },
                        { nameof(Event.Description), false },
                        { nameof(Event.MaxPairs), false },
                        { nameof(Event.MaxMen), false },
                        { nameof(Event.MaxWomen), false },
                        { nameof(Event.Country), false },
                        { nameof(Event.Country.Region), false },
                        { nameof(Event.Address), false }
                    } }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { "Schedule", false } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { "Photos", false } } } }
            };

            CheckPanelsVisibility();
        }


        async void AddAsync()
        {
            processingEvent = true;
            StateHasChanged();

            // Добавление мероприятия
            var request = new AddEventRequestDto { Event = Event, Token = CurrentState.Account?.Token };
            var apiAddResponse = await _repoAddEvent.HttpPostAsync(request);
            EventId = apiAddResponse.Response.NewEventId;

            // TODO Сделать переадресацию на страницу мероприятия после успешного добавления (OK)

            // Перезагрузка мероприятия
            var apiReloadResponse = await _repoGetEvent.HttpPostAsync(new GetEventsRequestDto { EventId = EventId, IsPhotosIncluded = true });
            Event = apiReloadResponse.Response.Event!;

            processingEvent = false;
            StateHasChanged();
        }
    }
}
