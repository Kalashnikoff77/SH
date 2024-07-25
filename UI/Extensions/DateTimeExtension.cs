﻿namespace UI.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Конвертация даты и времени в слова "сегодня, 10:15", "завтра, 20:00", "вчера, 09:45" или "27 дек. 2023, 13:00"
        /// </summary>
        public static string ToMyString(this DateTime dateTime)
        {
            var time = dateTime.ToString("HH:mm");
            var now = DateTime.Now;
            var yesterday = now.AddDays(-1);
            var tomorrow = now.AddDays(1);

            if (dateTime > now)
            {
                if (dateTime < now.AddMinutes(1))
                    return "прямо сейчас";

                if (dateTime < now.AddDays(2))
                {
                    if (dateTime.Day == now.Day)
                        return $"сегодня, {time}";
                    if (dateTime.Day == tomorrow.Day)
                        return $"завтра, {time}";
                }
            }
            else
            {
                if (dateTime > now.AddMinutes(-1))
                    return "только что";

                if (dateTime > now.AddDays(-2))
                {
                    if (dateTime.Day == now.Day)
                        return $"сегодня, {time}";
                    if (dateTime.Day == yesterday.Day)
                        return $"вчера, {time}";
                }
            }

            return dateTime.ToString("dd MMM yyyy, HH:mm");
        }
    }
}