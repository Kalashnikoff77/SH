using Common.Dto;
using Microsoft.AspNetCore.Components;

namespace Common.Extensions
{
    public static class SchedulesForEventsDtoExtension
    {
        /// <summary>
        /// Конвертация двух дат в слова "сегодня 10:15 - 20:00", "сегодня 10:15 - завтра 20:00", "пт, 10 сент. 20:00 - вс, 12 сент. 18:00"
        /// </summary>
        public static MarkupString ToStartEndString(this SchedulesForEventsDto schedule)
        {
            if (schedule.StartDate.Date != schedule.EndDate.Date)
                return new MarkupString($"{schedule.StartDate.ToMyString()} &mdash; {schedule.EndDate.ToMyString()}");
            else
                return new MarkupString($"{schedule.StartDate.ToMyString()} &mdash; {schedule.EndDate.ToString("HH:mm")}");
        }
    }
}
