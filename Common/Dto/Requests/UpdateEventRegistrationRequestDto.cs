namespace Common.Dto.Requests
{
    public class UpdateEventRegistrationRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/UpdateRegistration";

        public int ScheduleId { get; set; }
    }
}
