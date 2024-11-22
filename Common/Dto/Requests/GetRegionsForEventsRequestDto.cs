using System.Text.Json;

namespace Common.Dto.Requests
{
    public class GetRegionsForEventsRequestDto : RequestDtoBase
    {
        public override string Uri => "/Countries/GetRegionsForEvents";

        public int GetCacheKey() =>
            JsonSerializer.Serialize(this).GetHashCode();
    }
}
