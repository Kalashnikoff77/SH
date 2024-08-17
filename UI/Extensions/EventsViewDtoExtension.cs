using Common;
using Common.Dto;
using Common.Dto.Views;

namespace UI.Extensions
{
    public static class EventsViewDtoExtension
    {
        /// <summary>
        /// Конвертация страны и региона в текст
        /// </summary>
        /// <param name="finish">HTML, добавляемый в конец строки</param>
        /// <returns>"Россия, Москва"</returns>
        public static string ToRegionString(this EventsViewDto evt) =>
            $"{evt.Country!.Name}, {evt.Country.Region.Name}";


        public static string ToDateClass(this SchedulesForEventsDto evt)
        {
            string dateClass = null!;

            if (evt.StartDate < DateTime.Now && evt.EndDate < DateTime.Now)
                dateClass = "red-text";
            else if (evt.StartDate < DateTime.Now)
                dateClass = "orange-text";
            else if (evt.StartDate < DateTime.Now.AddDays(3))
                dateClass = "green-text";
            else if (evt.StartDate < DateTime.Now.AddDays(7))
                dateClass = "blue-text";

            return dateClass;
        }

        public static string ToShortDescription(this EventsViewDto evt)
        {
            string description;

            if (evt.Description.Length > 95)
                description = evt.Description.Substring(0, 95) + "...";
            else
                description = evt.Description;

            return description;
        }


        public static int NumberOfRegisteredMen(this EventsViewDto evt)
        {
            int count = 0;
            if (evt.RegisteredAccounts != null)
                count = evt.RegisteredAccounts.Where(w => w.UserGender == 0).Count();
            return count;
        }

        public static int NumberOfRegisteredWomen(this EventsViewDto evt)
        {
            int count = 0;
            if (evt.RegisteredAccounts != null)
                count = evt.RegisteredAccounts.Where(w => w.UserGender == 1).Count();
            return count;
        }

        public static int NumberOfRegisteredPairs(this EventsViewDto evt)
        {
            int count = 0;
            if (evt.RegisteredAccounts != null)
                count = evt.RegisteredAccounts.Where(w => w.UserGender == null).Count();
            return count;
        }

        public static int NumberOfRegisteredAll(this EventsViewDto evt)
        {
            int count = 0;
            if (evt.RegisteredAccounts != null)
                count = evt.RegisteredAccounts.Count();
            return count;
        }

        public static string? AverageAgeOfRegistered(this EventsViewDto evt)
        {
            if (evt.RegisteredAccounts != null)
            {
                var usersDates = evt.RegisteredAccounts.Select(s => s.Account.Users)
                    .Select(s => s.Select(s => s.BirthDate)).ToList();

                List<int> ages = new List<int>();

                foreach (var userDate in usersDates)
                {
                    foreach (var birthDate in userDate)
                    {
                        var age = DateTime.Today.Year - birthDate.Year;
                        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
                        ages.Add(age);
                    }
                }

                var averageIntAge = ages.Sum(s => s) / ages.Count;
                var lastDigit = averageIntAge % 10;
                string years = null!;

                switch (lastDigit)
                {
                    case 0: case 5: case 6: case 7: case 8: case 9: years = "лет"; break;
                    case 1: years = "год"; break;
                    case 2: case 3: case 4: years = "года"; break;
                }

                return "~" + averageIntAge + " " + years;
            }

            return null;
        }
    }
}
