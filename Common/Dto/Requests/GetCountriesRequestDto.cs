using System.Text.Json;

namespace Common.Dto.Requests
{
    public class GetCountriesRequestDto : RequestDtoBase
    {
        public override string Uri => "/Countries/Get";

        public int? CountryId { get; set; }

        public int GetCacheKey() =>
            JsonSerializer.Serialize(this).GetHashCode();
    }
}
