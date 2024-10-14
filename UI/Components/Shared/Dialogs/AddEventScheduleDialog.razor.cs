using Common.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
{
    public partial class AddEventScheduleDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

        SchedulesForEventsDto Schedule { get; set; } = new SchedulesForEventsDto();

        const int maxStartDateDays = 30 * 3;
        const int maxEndDateDays = 30;
        bool IsFormValid = false;
        public bool EventType { get; set; } = true;

        // Дни недели
        public List<bool> DaysOfWeek { get; set; } = new List<bool> { false, false, false, false, false, false, false };

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

        Color StartDateIconColor = Color.Default;
        Color StartTimeIconColor = Color.Default;

        DateTime? StartDate
        {
            get => Schedule.StartDate == DateTime.MinValue ? null : Schedule.StartDate;
            set { if (value != null) { Schedule.StartDate = value.Value; } }
        }
        TimeSpan? StartTime { get; set; }
        #endregion


        #region /// EndDate ///
        Color EndDateIconColor = Color.Default;
        Color EndTimeIconColor = Color.Default;
        string? EndDateValidator(DateTime? endDate)
        {
            string? errorMessage = null;
            if (endDate == null)
                errorMessage = $"Укажите дату окончания мероприятия";

            if (endDate.HasValue && (endDate < StartDate || endDate > StartDate!.Value.AddDays(maxEndDateDays)))
                errorMessage = $"Максимальная дата {StartDate.Value.AddDays(maxEndDateDays).ToString("dd.MM.yyyy")}";

            return errorMessage;
        }

        DateTime? EndDate
        {
            get => Schedule.EndDate == DateTime.MinValue ? null : Schedule.EndDate;
            set { if (value != null) { Schedule.EndDate = value.Value; } }
        }

        TimeSpan? EndTime { get; set; }
        #endregion


        void Submit() => MudDialog.Close(DialogResult.Ok(Schedule));

        void Cancel() => MudDialog.Cancel();
    }
}
