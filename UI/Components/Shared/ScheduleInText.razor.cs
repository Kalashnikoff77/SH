using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared
{
    public partial class ScheduleInText
    {
        [Parameter, EditorRequired] public bool IsOneTimeEvent { get; set; }
        [Parameter, EditorRequired] public DateTime? StartDate { get; set; }
        [Parameter, EditorRequired] public DateTime? EndDate { get; set; }
        [Parameter, EditorRequired] public TimeSpan? StartTime { get; set; }
        [Parameter, EditorRequired] public TimeSpan? EndTime { get; set; }
        [Parameter] public List<bool> DaysOfWeek { get; set; } = null!;

    }
}
