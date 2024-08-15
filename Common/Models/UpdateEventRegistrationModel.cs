namespace Common.Models
{
    public class UpdateEventRegistrationModel : ModelBase
    {
        public int EventId { get; set; }
        public bool ToRegister { get; set; }
    }
}
