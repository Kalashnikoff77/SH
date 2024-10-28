using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Enums;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using PhotoSauce.MagicScaler;
using System.Net;
using UI.Components.Dialogs;
using UI.Models;

namespace UI.Components.Pages.Events
{
    public partial class AddEditEvent : IDisposable
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Inject] IRepository<EventCheckRequestDto, EventCheckResponseDto> _repoCheckAdding { get; set; } = null!;
        [Inject] IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvent { get; set; } = null!;
        [Inject] IRepository<UpdateEventRequestDto, UpdateEventResponseDto> _repoUpdateEvent { get; set; } = null!;
        [Inject] IRepository<UploadEventPhotoFromTempRequestDto, UploadEventPhotoFromTempResponseDto> _repoUploadPhoto { get; set; } = null!;

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

        // Список загруженных фото во временный каталог
        List<Tuple<string, string>> photos = new List<Tuple<string, string>>();

        protected override async Task OnInitializedAsync()
        {
            if (EventId != null)
            {
                var apiResponse = await _repoGetEvent.HttpPostAsync(new GetEventsRequestDto { EventId = EventId, IsPhotosIncluded = true });
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

        async Task EditScheduleDialogAsync(SchedulesForEventsViewDto Schedule)
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

        async Task DeleteScheduleDialogAsync(SchedulesForEventsViewDto schedule)
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
        async void UploadPhotos(IReadOnlyList<IBrowserFile> browserPhotos)
        {
            if (browserPhotos.Count > 0)
            {
                processingPhoto = true;
                StateHasChanged();

                foreach (var photo in browserPhotos)
                {
                    var basePhoto = DateTime.Now.ToString("yyyyMMddmmss") + "_" + Guid.NewGuid().ToString();
                    var originalPhoto = basePhoto + Path.GetExtension(photo.Name);

                    await using (FileStream fs = new(StaticData.EventsPhotosTempDir + "/" + originalPhoto, FileMode.Create))
                        await photo.OpenReadStream(photo.Size).CopyToAsync(fs);

                    var previewPhoto = basePhoto + "_" + EnumImageSize.s150x150 + ".jpg";

                    using (MemoryStream output = new MemoryStream(500000))
                    {
                        MagicImageProcessor.ProcessImage(StaticData.EventsPhotosTempDir + "/" + originalPhoto, output, StaticData.Images[EnumImageSize.s150x150]);
                        await File.WriteAllBytesAsync(StaticData.EventsPhotosTempDir + "/" + previewPhoto, output.ToArray());
                    }

                    photos.Add(new Tuple<string, string>(originalPhoto, previewPhoto));
                    StateHasChanged();

                    if (photos.Count >= 20) break;
                }

                //var request = new UploadEventPhotoFromTempRequestDto
                //{
                //    EventId = Event.Id,
                //    Token = CurrentState.Account.Token
                //};

                //foreach (var photo in photos)
                //{
                //    var baseFileName = DateTime.Now.ToString("yyyyMMddmmss") + "_" + Guid.NewGuid().ToString();
                //    var originalFileName = baseFileName + Path.GetExtension(photo.Name);

                //    await using (FileStream fs = new(StaticData.EventsPhotosTempDir + originalFileName, FileMode.Create))
                //        await photo.OpenReadStream(photo.Size).CopyToAsync(fs);

                //    request.PhotosTempFileNames = originalFileName;

                //    var apiResponse = await _repoUploadPhoto.HttpPostAsync(request);

                //    Event.Photos.Add(apiResponse.Response.NewPhoto);
                //    StateHasChanged();

                //    if (Event.Photos.Count >= 20) break;
                //}
                processingPhoto = false;
                StateHasChanged();
            }
        }

        void DeleteAsync(Tuple<string, string> photo)
        {
            photos.Remove(photo);
            if (File.Exists(StaticData.EventsPhotosTempDir + "/" + photo.Item1))
                File.Delete(StaticData.EventsPhotosTempDir + "/" + photo.Item1);
            if (File.Exists(StaticData.EventsPhotosTempDir + "/" + photo.Item2))
                File.Delete(StaticData.EventsPhotosTempDir + "/" + photo.Item2);
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

            var request = new UpdateEventRequestDto { Event = Event, Token = CurrentState.Account?.Token };
            var response = await _repoUpdateEvent.HttpPostAsync(request);

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

        public void Dispose()
        {
            foreach (var photo in photos) 
            {
                if (File.Exists(StaticData.EventsPhotosTempDir + "/" + photo.Item1))
                    File.Delete(StaticData.EventsPhotosTempDir + "/" + photo.Item1);
                if (File.Exists(StaticData.EventsPhotosTempDir + "/" + photo.Item2))
                    File.Delete(StaticData.EventsPhotosTempDir + "/" + photo.Item2);
            }
        }
    }
}
