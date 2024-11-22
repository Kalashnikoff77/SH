﻿using System.Text.Json;

namespace Common.Dto.Requests
{
    public class GetFeaturesRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/GetFeatures";

        public int GetCacheKey() =>
            JsonSerializer.Serialize(this).GetHashCode();
    }
}
