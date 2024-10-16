using Common.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
{
    public partial class AddEventScheduleDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

        SchedulesForEventsDto schedule { get; set; } = new SchedulesForEventsDto();

        const int maxStartDateDays = 30 * 3;
        const int maxEndDateDays = 30;
        bool isFormValid = false;
        string? errorMessage;

        bool _isOneTimeEvent = true;
        bool isOneTimeEvent
        { 
            get => _isOneTimeEvent;
            set { _isOneTimeEvent = value; daysOfWeek = daysOfWeek.Select(f => f = false).ToArray(); CheckProperties(); }
        }

        // Дни недели
        bool[] daysOfWeek = { false, false, false, false, false, false, false };

        DateTime? startDate
        {
            get => schedule.StartDate == DateTime.MinValue ? null : schedule.StartDate;
            set { if (value != null) { schedule.StartDate = value.Value; CheckProperties(); } }
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
            set { if (value != null) { schedule.EndDate = value.Value; CheckProperties(); } }
        }
        TimeSpan? _endTime;
        TimeSpan? endTime
        {
            get => _endTime;
            set { _endTime = value!.Value; CheckProperties(); }
        }

        void OnWeekChanged(int weekDay, bool isChecked)
        {
            daysOfWeek[weekDay] = isChecked;
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
                        if (daysOfWeek.Any(a => a == true))
                        {
                            var listOfSchedulesResult = GetListOfSchedules(startDate.Value, startTime.Value, endDate.Value, endTime.Value, daysOfWeek);
                            if (listOfSchedulesResult.Item1)
                            {
                                isFormValid = true;
                            }
                        }
                    }
                }
            }
            StateHasChanged();
        }


        void Submit() => MudDialog.Close(DialogResult.Ok(schedule));

        void Cancel() => MudDialog.Cancel();


        Tuple<bool, List<SchedulesForEventsDto>> GetListOfSchedules(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, bool[] daysOfWeek)
        {
            var result = new Tuple<bool, List<SchedulesForEventsDto>>(false, new List<SchedulesForEventsDto>());

            var fullStartDate = startDate + startTime;
            var fullEndDate = endDate + endTime;

            for (DateTime curDate = fullStartDate; curDate <= fullEndDate; curDate = curDate.AddMinutes(5))
            {
                var test = (int)curDate.DayOfWeek;
            }

            return result!;
        }
    }
}
