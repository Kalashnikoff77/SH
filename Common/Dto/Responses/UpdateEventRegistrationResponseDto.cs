namespace Common.Dto.Responses
{
    public class UpdateEventRegistrationResponseDto : ResponseDtoBase
    {
        public int EventId { get; set; }
        public bool IsRegistered { get; set; } = false;
    }
}
