namespace Common.Dto.Requests
{
    public class UpdateEventRegistrationRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/UpdateRegistration";

        public int EventId { get; set; }
        public bool ToRegister { get; set; }
    }
}
