using System.Text.Json;

namespace Common.Dto.Requests
{
    public abstract class RequestDtoBase
    {
        public abstract string Uri { get; }

        public string? Token { get; set; }

        public int Take { get; set; } = 20;
        public int Skip { get; set; } = 0;

        public string? FilterFreeText { get; set; }

        public int GetCacheKey<T>(T request) =>
            JsonSerializer.Serialize(request).GetHashCode();
    }
}
