﻿namespace Common.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Конвертация даты и времени в слова "сегодня 10:15", "завтра 20:00", "вчера 09:45" или "пн, 27 дек. 2023 13:00"
        /// </summary>
        public static string ToMyString(this DateTime dateTime)
        {
            //dateTime = DateTime.Parse("31.08.2024 18:00");

            var time = dateTime.ToString("HH:mm");
            var now = DateTime.Now;
            var yesterday = now.AddDays(-1);
            var tomorrow = now.AddDays(1);
            var aftetomorrow = now.AddDays(2);

            if (dateTime.Date > now.Date)
            {
                if (dateTime < now.AddMinutes(1))
                    return "прямо сейчас";

                if (dateTime.Date <= now.AddDays(2).Date)
                {
                    if (dateTime.Date == now.Date)
                        return $"сегодня {time}";
                    if (dateTime.Date == tomorrow.Date)
                        return $"завтра {time}";
                    if (dateTime.Date == aftetomorrow.Date)
                        return $"послезавтра {time}";
                }
            }
            else
            {
                if (dateTime > now.AddMinutes(-1))
                    return "только что";

                if (dateTime.Date > now.AddDays(-2).Date)
                {
                    if (dateTime.Date == now.Date)
                        return $"сегодня {time}";
                    if (dateTime.Date == yesterday.Date)
                        return $"вчера {time}";
                }
            }

            if (dateTime.Year == DateTime.Now.Year)
                return dateTime.ToString("ddd, dd MMM HH:mm");
            else
                return dateTime.ToString("ddd, dd MMM yyyy, HH:mm");
        }

        /// <summary>
        /// Конвертация даты и времени в прошлом в короткие слова "13:45", "вчера", "1 мес" или "давно"
        /// </summary>
        public static string ToMyPastShortString(this DateTime dateTime)
        {
            var time = dateTime.ToString("HH:mm");
            var now = DateTime.Now;
            var yesterday = now.AddDays(-1);
            var tomorrow = now.AddDays(1);

            if (dateTime.Date > now.AddMinutes(-1))
                return "сейчас";

            if (dateTime.Date > now.AddDays(-2))
            {
                if (dateTime.Date == now.Date)
                    return time;
                if (dateTime.Date == yesterday.Date)
                    return "вчера";
            }

            for (var c = 2; c <= 30; c++)
            {
                if (dateTime > now.AddDays(-c))
                    return $"{c} дн";
            }

            for (var c = 1; c <= 12; c++)
            {
                if (dateTime > now.AddMonths(-c))
                    return $"{c} мес";
            }

            return "давно";
        }
    }
}
