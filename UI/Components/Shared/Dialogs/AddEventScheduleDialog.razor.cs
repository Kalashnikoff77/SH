using Common.Dto;
using Common.Dto.Views;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
{
    public partial class AddEventScheduleDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public EventsViewDto Event { get; set; } = null!;

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

        DateTime? _startDate;
        DateTime? startDate
        {
            get => _startDate == DateTime.MinValue ? null : _startDate;
            set { if (value != null) { _startDate = value.Value; CheckProperties(); } }
        }
        TimeSpan? _startTime;
        TimeSpan? startTime
        {
            get => _startTime; 
            set { _startTime = value!.Value; CheckProperties(); }
        }

        DateTime? _endDate;
        DateTime? endDate
        {
            get => _endDate == DateTime.MinValue ? null : _endDate; 
            set { if (value != null) { _endDate = value.Value; CheckProperties(); } }
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
                        isFormValid = true;
                    }
                    else
                    {
                        if (daysOfWeek.Count > 0)
                        {
                            var listOfSchedulesResult = GetListOfSchedules(startDate.Value, startTime.Value, endDate.Value, endTime.Value, daysOfWeek);
                            if (listOfSchedulesResult.Count > 0)
                                isFormValid = true;
                            else
                                errorMessage = "В указанный период ни одно мероприятие не попадает.";
                        }
                    }
                }
            }
            StateHasChanged();
        }


        void Submit() => MudDialog.Close(DialogResult.Ok(schedules));
        void Cancel() => MudDialog.Cancel();


        List<SchedulesForEventsDto> GetListOfSchedules(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, HashSet<short> daysOfWeek)
        {
            var result = new List<SchedulesForEventsDto>();

            for (DateTime curDate = startDate; curDate <= endDate; curDate = curDate.AddDays(1))
            {
                if (daysOfWeek.Contains((short)curDate.DayOfWeek))
                {
                    result.Add(new SchedulesForEventsDto
                    {
                        EventId = Event.Id,
                        Description = "Description",
                        StartDate = curDate + startTime,
                        EndDate = curDate + endTime,
                        CostMan = 0,
                        CostWoman = 0,
                        CostPair = 0
                    });
                }
            }

            return result;
        }
    }
}
