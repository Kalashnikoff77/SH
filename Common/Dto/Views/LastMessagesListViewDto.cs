namespace Common.Dto.Views
{
    public class LastMessagesListViewDto : DtoBase
    {
        public DateTime CreateDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public string? Text { get; set; }

        public AccountsViewDto? Sender { get; set; }
        public AccountsViewDto? Recipient { get; set; }
    }
}
