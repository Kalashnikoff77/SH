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
        public bool isOneTimeEvent { get; set; } = true;

        // Дни недели
        public List<bool> daysOfWeek { get; set; } = new List<bool> { false, false, false, false, false, false, false };

        #region /// StartDate ///
        string? StartDateValidator(DateTime? startDate)
        {
            string? errorMessage = null;
            if (startDate == null)
                errorMessage = $"Укажите дату начала мероприятия";

            if (startDate.HasValue && (startDate < DateTime.Now || startDate > DateTime.Now.AddDays(maxStartDateDays)))
                errorMessage = $"Максимальная дата {DateTime.Now.AddDays(maxStartDateDays).ToString("dd.MM.yyyy")}";

            return errorMessage;
        }

        Color startDateIconColor = Color.Default;
        Color startTimeIconColor = Color.Default;

        DateTime? startDate
        {
            get => schedule.StartDate == DateTime.MinValue ? null : schedule.StartDate;
            set { if (value != null) { schedule.StartDate = value.Value; } }
        }
        TimeSpan? startTime { get; set; }
        #endregion


        #region /// EndDate ///
        Color endDateIconColor = Color.Default;
        Color endTimeIconColor = Color.Default;
        string? EndDateValidator(DateTime? endDate)
        {
            string? errorMessage = null;
            if (endDate == null)
                errorMessage = $"Укажите дату окончания мероприятия";

            if (endDate.HasValue && (endDate < startDate || endDate > startDate!.Value.AddDays(maxEndDateDays)))
                errorMessage = $"Максимальная дата {startDate.Value.AddDays(maxEndDateDays).ToString("dd.MM.yyyy")}";

            return errorMessage;
        }

        DateTime? endDate
        {
            get => schedule.EndDate == DateTime.MinValue ? null : schedule.EndDate;
            set { if (value != null) { schedule.EndDate = value.Value; } }
        }

        TimeSpan? endTime { get; set; }
        #endregion


        void Submit() => MudDialog.Close(DialogResult.Ok(schedule));

        void Cancel() => MudDialog.Cancel();
    }
}
