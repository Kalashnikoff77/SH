namespace Common.Dto.Views
{
    public class RegionsForEventsViewDto : RegionsDto
    {
        public int NumberOfEvents { get; set; }

        /// <summary>
        /// Для корректного вывода названия региона в выпадающем меню в Events.razor.cs
        /// </summary>
        public override string ToString() => Name;
    }
}
