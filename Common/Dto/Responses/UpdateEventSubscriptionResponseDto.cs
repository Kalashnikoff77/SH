namespace Common.Dto.Responses
{
    public class UpdateEventSubscriptionResponseDto : ResponseDtoBase
    {
        public int EventId { get; set; }
        public bool IsSubscribed { get; set; } = false;
        public bool IsRegistered { get; set; } = false;
    }
}
