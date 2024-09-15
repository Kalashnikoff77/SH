namespace Common.Dto.Views
{
    public class FeaturesForEventsViewDto : FeaturesDto
    {
        public int NumberOfEvents { get; set; }

        /// <summary>
        /// Для корректного вывода названия услуги в выпадающем меню в Events.razor.cs
        /// </summary>
        public override string ToString() => Name;
    }
}
