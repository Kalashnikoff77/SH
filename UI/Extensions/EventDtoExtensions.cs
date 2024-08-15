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


        public static string ToDateClass(this EventsSchedulesDto evt) 
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

    }
}
