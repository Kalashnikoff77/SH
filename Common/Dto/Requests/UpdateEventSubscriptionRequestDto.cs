namespace Common.Dto.Requests
{
    public class UpdateEventSubscriptionRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/UpdateSubscription";

        public int EventId { get; set; }
        public bool ToSubscribe { get; set; }
        public bool ToRegister { get; set; }
    }
}
