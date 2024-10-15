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
        public bool isOneTimeEvent { get; set; } = true;

        // Дни недели
        public List<bool> daysOfWeek { get; set; } = new List<bool> { false, false, false, false, false, false, false };

        string? DateValidator(DateTime? startDate)
        {
            CheckProperties();
            return null;
        }

        string? TimeValidator(TimeSpan? startTime)
        {
            CheckProperties();
            return null;
        }

        DateTime? startDate
        {
            get => schedule.StartDate == DateTime.MinValue ? null : schedule.StartDate;
            set { if (value != null) { schedule.StartDate = value.Value; } }
        }
        TimeSpan? startTime { get; set; }


        DateTime? endDate
        {
            get => schedule.EndDate == DateTime.MinValue ? null : schedule.EndDate;
            set { if (value != null) { schedule.EndDate = value.Value; } }
        }

        TimeSpan? endTime { get; set; }


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
                        if (daysOfWeek != null && daysOfWeek.Any(a => a == true))
                        {
                            isFormValid = true;
                        }
                    }
                }
            }
            StateHasChanged();
        }


        void Submit() => MudDialog.Close(DialogResult.Ok(schedule));

        void Cancel() => MudDialog.Cancel();
    }
}
