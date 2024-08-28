namespace Common.Extensions
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
            var aftetomorrow = now.AddDays(2);

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
                    if (dateTime.Day == aftetomorrow.Day)
                        return $"послезавтра, {time}";
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

            if (dateTime.Year == DateTime.Now.Year)
                return dateTime.ToString("dd MMM HH:mm");
            else
                return dateTime.ToString("dd MMM yyyy, HH:mm");
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

            if (dateTime > now.AddMinutes(-1))
                return "сейчас";

            if (dateTime > now.AddDays(-2))
            {
                if (dateTime.Day == now.Day)
                    return time;
                if (dateTime.Day == yesterday.Day)
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
