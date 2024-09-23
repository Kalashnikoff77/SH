namespace Common.Dto.Requests
{
    public class GetScheduleOneRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/GetOne";

        public int ScheduleId { get; set; }
    }
}
