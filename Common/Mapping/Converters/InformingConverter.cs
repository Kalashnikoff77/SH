using AutoMapper;
using Common.Dto;
using System.Text.Json;

namespace Common.Mapping.Converters
{
    public class InformingConverter : IValueConverter<string?, Informing>
    {
        public Informing Convert(string? source, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source))
                return new Informing();
            
            var informing = JsonSerializer.Deserialize<Informing>(source);
            return informing!;
        }
    }
}
