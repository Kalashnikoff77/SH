using Microsoft.AspNetCore.Components;
using System.Text;

namespace UI.Components.Pages.Events
{
    public partial class ScheduleToText
    {
        [Parameter, EditorRequired] public bool IsOneTimeEvent { get; set; }
        [Parameter, EditorRequired] public DateTime? StartDate { get; set; }
        [Parameter, EditorRequired] public DateTime? EndDate { get; set; }
        [Parameter, EditorRequired] public TimeSpan? StartTime { get; set; }
        [Parameter, EditorRequired] public TimeSpan? EndTime { get; set; }
        [Parameter] public List<bool> DaysOfWeek { get; set; } = null!;

        StringBuilder result { get; set; } = new StringBuilder(100);

        protected override void OnParametersSet()
        {
            result.Clear();

            if (IsOneTimeEvent) 
            {
                if (StartDate.HasValue)
                    result.Append($"Начало {StartDate.Value.ToString("dd.MM.yyyy")}");
                if (StartTime.HasValue)
                    result.Append($" в {string.Format("{0:D2}:{1:D2}", StartTime.Value.Hours, StartTime.Value.Minutes)}");
                if (EndDate.HasValue)
                    result.Append($", завершение {EndDate.Value.ToString("dd.MM.yyyy")}");
                if (EndTime.HasValue)
                    result.Append($" в {string.Format("{0:D2}:{1:D2}", EndTime.Value.Hours, EndTime.Value.Minutes)}");
            } 
            else
            {
                if (StartDate.HasValue)
                    result.Append($"Период с {StartDate.Value.ToString("dd.MM.yyyy")}");
                if (EndDate.HasValue)
                    result.Append($" по {EndDate.Value.ToString("dd.MM.yyyy")}");

                if (StartTime.HasValue)
                    result.Append($", с {string.Format("{0:D2}:{1:D2}", StartTime.Value.Hours, StartTime.Value.Minutes)}");

                if (EndTime.HasValue)
                    result.Append($" до {string.Format("{0:D2}:{1:D2}", EndTime.Value.Hours, EndTime.Value.Minutes)}");

                if (DaysOfWeek != null && DaysOfWeek.Any(a => a == true)) 
                {
                    result.Append(", по ");
                    
                    if (DaysOfWeek[0]) result.Append("Пн, ");
                    if (DaysOfWeek[1]) result.Append("Вт, ");
                    if (DaysOfWeek[2]) result.Append("Ср, ");
                    if (DaysOfWeek[3]) result.Append("Чт, ");
                    if (DaysOfWeek[4]) result.Append("Пт, ");
                    if (DaysOfWeek[5]) result.Append("Сб, ");
                    if (DaysOfWeek[6]) result.Append("Вс, ");

                    result.Remove(result.Length - 2, 2);
                }
            }

        }
    }
}
