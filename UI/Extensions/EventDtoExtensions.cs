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

        public static string ToDateClass(this EventsViewDto evt) 
        {
            string dateClass = null!;

            if (evt.StartDate < DateTime.Now && evt.EndDate < DateTime.Now)
                dateClass = "rz-color-danger";
            else if (evt.StartDate < DateTime.Now)
                dateClass = "rz-color-warning-dark";
            else if (evt.StartDate < DateTime.Now.AddDays(3))
                dateClass = "rz-color-success-dark";
            else if (evt.StartDate < DateTime.Now.AddDays(7))
                dateClass = "rz-color-series-7";

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
