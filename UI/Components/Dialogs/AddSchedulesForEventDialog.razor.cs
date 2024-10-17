using Common.Dto;
using Common.Dto.Views;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Dialogs
{
    public partial class AddSchedulesForEventDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public EventsViewDto Event { get; set; } = null!;

        SchedulesForEventsDto schedule { get; set; } = new SchedulesForEventsDto();

        List<SchedulesForEventsDto> schedules { get; set; } = new List<SchedulesForEventsDto>();

        const int maxStartDateDays = 30 * 3;
        const int maxEndDateDays = 30;
        bool isFormValid = false;
        string? errorMessage;

        bool _isOneTimeEvent = true;
        bool isOneTimeEvent
        { 
            get => _isOneTimeEvent;
            set { _isOneTimeEvent = value; daysOfWeek.Clear(); CheckProperties(); }
        }

        // Дни недели
        HashSet<short> daysOfWeek = new HashSet<short>(7);

        DateTime? startDate
        {
            get => schedule.StartDate == DateTime.MinValue ? null : schedule.StartDate;
            set { schedule.StartDate = value!.Value; CheckProperties(); }
        }
        TimeSpan? _startTime;
        TimeSpan? startTime
        {
            get => _startTime; 
            set { _startTime = value!.Value; CheckProperties(); }
        }

        DateTime? endDate
        {
            get => schedule.EndDate == DateTime.MinValue ? null : schedule.EndDate; 
            set { schedule.EndDate = value!.Value; CheckProperties(); }
        }
        TimeSpan? _endTime;
        TimeSpan? endTime
        {
            get => _endTime;
            set { _endTime = value!.Value; CheckProperties(); }
        }

        void OnWeekChanged(short weekDay, bool isChecked)
        {
            if (isChecked)
                daysOfWeek.Add(weekDay);
            else
                daysOfWeek.Remove(weekDay);
            CheckProperties();
        }


        void CheckProperties()
        {
            isFormValid = false;
            errorMessage = null;

            if (startDate.HasValue && startTime.HasValue && endDate.HasValue && endTime.HasValue)
            {
                if (startDate.Value + startTime.Value >= endDate.Value + endTime.Value)
                {
                    errorMessage = "Дата начала мероприятия должна быть меньше даты его окончания";
                }
                else
                {
                    if (isOneTimeEvent)
                    {
                        schedules.Clear();
                        schedules.Add(new SchedulesForEventsDto
                        {
                            EventId = Event.Id,
                            Description = schedule.Description,
                            StartDate = startDate.Value + startTime.Value,
                            EndDate = endDate.Value + endTime.Value,
                            CostMan = schedule.CostMan,
                            CostWoman = schedule.CostWoman,
                            CostPair = schedule.CostPair
                        });
                        isFormValid = true;
                    }
                    else
                    {
                        if (daysOfWeek.Count > 0)
                        {
                            var numOfSchedules = GetListOfSchedules(startDate.Value, startTime.Value, endDate.Value, endTime.Value, daysOfWeek);
                            if (numOfSchedules.Count > 0)
                                isFormValid = true;
                            else
                                errorMessage = $"В период с {startDate.Value.ToString("dd.MM.yyyy")} по {endDate.Value.ToString("dd.MM.yyyy")} ни одно мероприятие не попадает.";
                        }
                    }
                }
            }
            StateHasChanged();
        }


        void Submit()
        {
            // Финальная проверка перед закрытием окна
            CheckProperties();
            if (isFormValid)
                MudDialog.Close(DialogResult.Ok(schedules));
        }
        void Cancel() => MudDialog.Cancel();


        List<SchedulesForEventsDto> GetListOfSchedules(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, HashSet<short> daysOfWeek)
        {
            schedules.Clear();

            for (DateTime curDate = startDate; curDate <= endDate; curDate = curDate.AddDays(1))
            {
                if (daysOfWeek.Contains((short)curDate.DayOfWeek))
                {
                    schedules.Add(new SchedulesForEventsDto
                    {
                        EventId = Event.Id,
                        Description = schedule.Description,
                        StartDate = curDate + startTime,
                        EndDate = curDate + endTime,
                        CostMan = schedule.CostMan,
                        CostWoman = schedule.CostWoman,
                        CostPair = schedule.CostPair
                    });
                }
            }
            return schedules;
        }
    }
}
