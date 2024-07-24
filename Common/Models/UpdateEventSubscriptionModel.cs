namespace Common.Models
{
    public class UpdateEventSubscriptionModel : ModelBase
    {
        public int EventId { get; set; }
        public bool ToSubscribe { get; set; }
        public bool ToRegister { get; set; }
    }
}
