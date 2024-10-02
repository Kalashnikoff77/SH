namespace Common.Dto.Responses
{
    public class UpdateEventRegistrationResponseDto : ResponseDtoBase
    {
        public int ScheduleId { get; set; }
        
        public bool IsRegistered { get; set; } = false;
    }
}
