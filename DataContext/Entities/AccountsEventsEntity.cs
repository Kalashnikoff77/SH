namespace DataContext.Entities
{
    public class AccountsEventsEntity : EntityBase
    {
        public int AccountId { get; set; }

        public int EventId { get; set; }

        public bool IsSubscribed { get; set; }
        public bool IsRegistered { get; set; }
    }
}
