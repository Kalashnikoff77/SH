using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;
using UI.Components.Dialogs;
using UI.Models;

namespace UI.Components.Pages.Events
{
    public partial class AddEdit
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<EventCheckRequestDto, EventCheckResponseDto> _repoCheckAdding { get; set; } = null!;
        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvent { get; set; } = null!;
        [Inject] IRepository<UpdateEventRequestDto, UpdateEventResponseDto> _repoAddSchedules { get; set; } = null!;

        [Inject] IDialogService DialogService { get; set; } = null!;
        [Parameter] public int? EventId { get; set; }

        EventsViewDto Event = new EventsViewDto() 
        {
            Name = "Название мероприятия в клубе",
            Description = "Длинное описание, которое должно быть более пятидесяти символов в длину, иначе не прокатит",
            MaxPairs = 10,
            MaxMen = 5,
            MaxWomen = 15
        };

        bool processingPhoto, processingEvent = false;

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool IsPanel1Valid, IsPanel2Valid, IsPanel3Valid, isValid;

        protected override async Task OnInitializedAsync()
        {
            if (EventId != null)
            {
                var apiResponse = await _repoGetEvent.HttpPostAsync(new GetEventsRequestDto { EventId = EventId });
                if (apiResponse.StatusCode == HttpStatusCode.OK && apiResponse.Response.Event != null)
                {
                    Event = apiResponse.Response.Event;
                    isValid = true;
                }
                else
                {
                    EventId = null;
                    isValid = false;
                }
            }

            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, bool>
                    {
                        { nameof(Event.Name), isValid },
                        { nameof(Event.Description), isValid },
                        { nameof(Event.MaxPairs), isValid },
                        { nameof(Event.MaxMen), isValid },
                        { nameof(Event.MaxWomen), isValid}
                    } }
                },
                { 2, new TabPanel { Items = new Dictionary<string, bool> { { "Schedule", isValid } } } },
                { 3, new TabPanel { Items = new Dictionary<string, bool> { { "Photo", isValid } } } }
            };

            CheckPanelsVisibility();
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
                var apiResponse = await _repoCheckAdding.HttpPostAsync(new EventCheckRequestDto { EventId = EventId, EventName = text, Token = CurrentState.Account!.Token });
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
        async Task AddScheduleDialogAsync()
        {
            var parameters = new DialogParameters<AddScheduleForEventDialog> 
            {
                { x => x.Event, Event }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var resultDialog = await DialogService.ShowAsync<AddScheduleForEventDialog>("Добавление расписания", parameters, options);
            var result = await resultDialog.Result;

            if (result != null && result.Canceled == false && result.Data != null)
            {
                if (Event.Schedule == null)
                    Event.Schedule = new List<SchedulesForEventsDto>();

                Event.Schedule.AddRange((List<SchedulesForEventsDto>)result.Data);
            }

            CheckPanel2Properties();
        }

        async Task EditScheduleDialogAsync(SchedulesForEventsDto Schedule)
        {
            var parameters = new DialogParameters<EditScheduleForEventDialog>
            {
                { x => x.Schedule, Schedule }
            };
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var resultDialog = await DialogService.ShowAsync<EditScheduleForEventDialog>("Редактирование расписания", parameters, options);
            var result = await resultDialog.Result;

            if (result != null && result.Canceled == false && result.Data != null)
            {
                Schedule = (SchedulesForEventsDto)result.Data;

                //if (Event.Schedule == null)
                //    Event.Schedule = new List<SchedulesForEventsDto>();

                //Event.Schedule.AddRange((List<SchedulesForEventsDto>)result.Data);
            }
        }

        async Task DeleteScheduleDialogAsync(SchedulesForEventsDto schedule)
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
                schedule.IsDeleted = true;

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


        #region /// ДОБАВЛЕНИЕ / УДАЛЕНИЕ ///
        async void AddAsync()
        {
            processingEvent = true;
            StateHasChanged();

            processingEvent = false;
            StateHasChanged();
        }

        async void UpdateAsync()
        {
            processingEvent = true;
            StateHasChanged();

            processingEvent = false;
            StateHasChanged();
        }
        #endregion


        void CheckPanelsVisibility()
        {
            IsPanel1Valid = TabPanels[1].Items.All(x => x.Value == true);
            IsPanel2Valid = TabPanels[2].Items.All(x => x.Value == true);
            IsPanel3Valid = TabPanels[3].Items.All(x => x.Value == true);
            StateHasChanged();
        }

    }
}
