namespace DataContext.Entities
{
    public class AccountsVisitsEntity : EntityBase
    {
        public DateTime CreateDate { get; set; }

        public DateTime LastDate { get; set; }

        public int AccountId { get; set; }
    }
}
