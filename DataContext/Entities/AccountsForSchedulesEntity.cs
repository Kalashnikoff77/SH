namespace DataContext.Entities
{
    public class AccountsForSchedulesEntity : EntityBase
    {
        public int ScheduleId { get; set; }
        public int AccountId { get; set; }
        public short? UserGender { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int TicketCost { get; set; }
        public bool IsPaid { get; set; }
    }
}
