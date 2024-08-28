namespace Common.Dto.Requests
{
    public class GetEventOneRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/GetOne";

        public int ScheduleId { get; set; }
    }
}
