using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Extensions;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using UI.Components.Dialogs;
using UI.Models;

namespace UI.Components.Pages.Events
{
    public abstract class EventDtoBase : ComponentBase
    {
        [CascadingParameter] protected CurrentState CurrentState { get; set; } = null!;
        [Inject] protected IRepository<GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] protected IRepository<EventCheckRequestDto, EventCheckResponseDto> _repoCheckAdding { get; set; } = null!;
        [Inject] protected IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvent { get; set; } = null!;
        [Inject] protected IRepository<AddEventRequestDto, AddEventResponseDto> _repoAddEvent { get; set; } = null!;
        [Inject] protected IRepository<UpdateEventRequestDto, UpdateEventResponseDto> _repoUpdateEvent { get; set; } = null!;
        [Inject] protected IRepository<UploadPhotoToTempRequestDto, UploadPhotoToTempResponseDto> _repoUploadPhotoToTemp { get; set; } = null!;

        [Inject] protected IDialogService DialogService { get; set; } = null!;

        List<CountriesViewDto> countries { get; set; } = null!;
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        protected bool processingPhoto, processingEvent, isDataSaved = false;

        protected Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        protected bool IsPanel1Valid, IsPanel2Valid, IsPanel3Valid, isValid;

        [Parameter] public int? EventId { get; set; }

        EventsViewDto Event = new EventsViewDto()
        {
            Name = "Название мероприятия в клубе",
            Description = "Длинное описание, которое должно быть более пятидесяти символов в длину, иначе не прокатит",
            Address = "МО, пос. Каменка, д. 12",
            MaxPairs = 10,
            MaxMen = 5,
            MaxWomen = 15
        };


        #region /// 1. ОБЩЕЕ ///
        public Color NameIconColor = Color.Default;
        public async Task<string?> NameValidator(string? text)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(text) || text.Length < StaticData.DB_EVENT_NAME_MIN)
            {
                errorMessage = $"Введите не менее {StaticData.DB_EVENT_NAME_MIN} символов";
            }
            else
            {
                var apiResponse = await _repoCheckAdding.HttpPostAsync(new EventCheckRequestDto { EventId = EventId, EventName = text, Token = CurrentState.Account!.Token });
                if (apiResponse.Response.EventNameExists)
                    errorMessage = $"Это имя уже занято. Выберите другое.";
            }

            CheckPanel1Properties(errorMessage, nameof(Event.Name), ref NameIconColor);
            return errorMessage;
        }

        public Color DescriptionIconColor = Color.Default;
        public string? DescriptionValidator(string? text)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(text) || text.Length < StaticData.DB_EVENT_DESCRIPTION_MIN)
                errorMessage = $"Введите не менее {StaticData.DB_EVENT_DESCRIPTION_MIN} символов";

            CheckPanel1Properties(errorMessage, nameof(Event.Description), ref DescriptionIconColor);
            return errorMessage;
        }

        public Color CountryIconColor = Color.Default;
        public string? CountryValidator(string country)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(countryText))
                errorMessage = $"Выберите страну";

            // Сбросим в false регион
            TabPanels[1].Items[nameof(Event.Country.Region)] = false;

            CheckPanel1Properties(errorMessage, nameof(Event.Country), ref CountryIconColor);
            return errorMessage;
        }

        public Color RegionIconColor = Color.Default;
        public string? RegionValidator(string region)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(regionText))
                errorMessage = $"Выберите регион";

            CheckPanel1Properties(errorMessage, nameof(Event.Country.Region), ref RegionIconColor);
            return errorMessage;
        }

        public Color AddressIconColor = Color.Default;
        public string? AddressValidator(string? address)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(address))
                errorMessage = $"Укажите точный или примерный адрес";

            CheckPanel1Properties(errorMessage, nameof(Event.Address), ref AddressIconColor);
            return errorMessage;
        }

        public async Task<IEnumerable<string>?> SearchCountry(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
                return countries.Select(s => s.Name);

            return countries?.Select(s => s.Name)
                .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<IEnumerable<string>?> SearchRegion(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value))
                return regions?.Select(s => s.Name);

            return regions?.Select(s => s.Name)
                .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }


        string? _countryText;
        public string? countryText
        {
            get => _countryText;
            set
            {
                if (value != null)
                {
                    var country = countries.Where(c => c.Name == value)?.First();
                    if (country != null)
                    {
                        if (Event.Country == null)
                            Event.Country = new CountriesDto();
                        Event.Country.Id = country.Id;
                        regions = countries
                            .Where(x => x.Id == country.Id).FirstOrDefault()?
                            .Regions?.Select(s => s).ToList();
                    }
                }
                _countryText = value;
                _regionText = null;
            }
        }

        string? _regionText;
        public string? regionText
        {
            get => _regionText;
            set
            {
                if (value != null && regions != null)
                {
                    var region = regions.Where(c => c.Name == value)?.First();
                    if (region != null)
                        Event.Country!.Region.Id = region.Id;
                }
                _regionText = value;
            }
        }


        public Color MaxPairsIconColor = Color.Default;
        public string? MaxPairsValidator(short? num)
        {
            string? errorMessage = null;
            if (!num.HasValue)
                errorMessage = "Укажите значение от 0 до 500";
            if (num < 0 || num > 500)
                errorMessage = "Кол-во от 1 до 500";

            CheckPanel1Properties(errorMessage, nameof(Event.MaxPairs), ref MaxPairsIconColor);
            return errorMessage;
        }

        public Color MaxMenIconColor = Color.Default;
        public string? MaxMenValidator(short? num)
        {
            string? errorMessage = null;
            if (!num.HasValue)
                errorMessage = "Укажите значение от 0 до 500";
            if (num < 0 || num > 500)
                errorMessage = "Кол-во от 1 до 500";

            CheckPanel1Properties(errorMessage, nameof(Event.MaxMen), ref MaxMenIconColor);
            return errorMessage;
        }

        public Color MaxWomenIconColor = Color.Default;
        public string? MaxWomenValidator(short? num)
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
                TabPanels[1].Items[property] = true;
                iconColor = Color.Success;
            }
            else
            {
                TabPanels[1].Items[property] = false;
                iconColor = Color.Error;
            }

            CheckPanelsVisibility();
        }
        #endregion


        #region /// 2. РАСПИСАНИЕ ///
        public async Task AddScheduleDialogAsync()
        {
            var parameters = new DialogParameters<AddScheduleForEventDialog>
            {
                { x => x.Event, Event }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var resultDialog = await DialogService.ShowAsync<AddScheduleForEventDialog>("Добавление расписания", parameters, options);
            var result = await resultDialog.Result;

            if (result != null && result.Canceled == false && result.Data != null)
            {
                if (Event.Schedule == null)
                    Event.Schedule = new List<SchedulesForEventsViewDto>();

                Event.Schedule.AddRange((List<SchedulesForEventsViewDto>)result.Data);
            }

            CheckPanel2Properties();
        }

        public async Task EditScheduleDialogAsync(SchedulesForEventsViewDto Schedule)
        {
            var parameters = new DialogParameters<EditScheduleForEventDialog>
            {
                { x => x.Schedule, Schedule }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

            var result = await (await DialogService.ShowAsync<EditScheduleForEventDialog>("Редактирование расписания", parameters, options)).Result;
            if (result != null && result.Canceled == false && result.Data != null)
            {
                var resultData = (SchedulesForEventsViewDto)result.Data;
                var index = Event.Schedule!.FindIndex(i => i.Id == resultData.Id);
                if (index > -1)
                    Event.Schedule[index] = resultData;

                CheckPanel2Properties();
            }
        }

        public async Task DeleteScheduleDialogAsync(SchedulesForEventsViewDto schedule)
        {
            var parameters = new DialogParameters<ConfirmDialog>
            {
                { x => x.ContentText, $"Удалить расписание?" },
                { x => x.ButtonText, "Удалить" },
                { x => x.Color, Color.Error }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var resultDialog = await DialogService.ShowAsync<ConfirmDialog>($"Удаление", parameters, options);
            var result = await resultDialog.Result;

            if (result != null && result.Canceled == false)
            {
                if (schedule.Id == 0)
                    Event.Schedule?.Remove(schedule);
                else
                    schedule.IsDeleted = true;
            }

            CheckPanel2Properties();
        }

        void CheckPanel2Properties()
        {
            // Есть ли хоть одно расписание активное (неудалённое)
            if (Event.Schedule?.Any(a => a.IsDeleted == false) == true)
                TabPanels[2].Items["Schedule"] = true;
            else
                TabPanels[2].Items["Schedule"] = false;

            CheckPanelsVisibility();
        }
        #endregion


        #region /// 3. ФОТО ///
        public async void UploadPhotos(IReadOnlyList<IBrowserFile> browserPhotos)
        {
            if (browserPhotos.Count > 0)
            {
                processingPhoto = true;
                StateHasChanged();

                if (Event.Photos == null)
                    Event.Photos = new List<PhotosForEventsDto>();

                foreach (var photo in browserPhotos)
                {
                    var newPhoto = await photo.Upload<PhotosForEventsDto>(CurrentState.Account?.Token, _repoUploadPhotoToTemp, eventId: Event.Id);

                    if (newPhoto != null)
                    {
                        // Если это первая фотка, то отметим её как аватар
                        if (Event.Photos.Count(x => x.IsDeleted == false) == 0)
                            newPhoto.IsAvatar = true;
                        Event.Photos.Insert(0, newPhoto);
                    }

                    StateHasChanged();
                    if (Event.Photos.Count(x => x.IsDeleted == false) >= 20) break;
                }

                processingPhoto = false;
                StateHasChanged();
            }

            CheckPanel3Properties();
        }

        public void UpdateCommentPhoto(PhotosForEventsDto photo, string comment) =>
            photo.Comment = comment;

        public void SetAsAvatarPhoto(PhotosForEventsDto photo)
        {
            Event.Photos?.ForEach(x => x.IsAvatar = false);
            photo.IsAvatar = true;
        }

        void CheckPanel3Properties()
        {
            if (Event.Photos?.Any(x => x.IsDeleted == false) == true)
                TabPanels[3].Items["Photos"] = true;
            else
                TabPanels[3].Items["Photos"] = false;

            CheckPanelsVisibility();
        }
        #endregion


        void CheckPanelsVisibility()
        {
            IsPanel1Valid = TabPanels[1].Items.All(x => x.Value == true);
            IsPanel2Valid = TabPanels[2].Items.All(x => x.Value == true);
            IsPanel3Valid = TabPanels[3].Items.All(x => x.Value == true);
            StateHasChanged();
        }

        public void Dispose()
        {
            if (Event.Photos != null)
                foreach (var photo in Event.Photos.Where(w => w.Id == 0))
                    if (Directory.Exists(StaticData.TempPhotosDir + "/" + photo.Guid))
                        Directory.Delete(StaticData.TempPhotosDir + "/" + photo.Guid, true);
        }
    }
}
