namespace Common.Dto
{
    public class AccountsDto : DtoBase
    {
        public Guid Guid { get; set; }

        public string? Email { get; set; }

        public string Name { get; set; } = null!;

        public string? Password { get; set; }

        public string? Informing { get; set; } = null!;

        public bool? IsConfirmed { get; set; }

        public string? Token { get; set; }

        /// <summary>
        /// Для корректного вывода названия региона в выпадающем меню в Events.razor.cs
        /// </summary>
        public override string ToString() => Name;
    }

    public class Informing
    {
        public bool IsNotification { get; set; } = true;
        public bool IsMessage { get; set; } = true;
    }
}
