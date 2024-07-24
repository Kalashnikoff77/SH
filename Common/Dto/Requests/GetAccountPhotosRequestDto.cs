namespace Common.Dto.Requests
{
    public class GetAccountPhotosRequestDto : RequestDtoBase
    {
        public override string Uri => "/Photos/Get";

        public int? AccountId { get; set; }

        public int Take { get; set; } = 20;
    }
}
