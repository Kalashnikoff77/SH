using AutoMapper;
using Common.Dto;
using System.Text.Json;

namespace WebAPI.Mapping.Converters
{
    public class StringToInformingConverter : IValueConverter<string, Informing>
    {
        public Informing Convert(string source, ResolutionContext context)
        {
            var informing = JsonSerializer.Deserialize<Informing>(source);
            return informing!;
        }
    }

    public class InformingToStringConverter : IValueConverter<Informing, string>
    {
        public string Convert(Informing source, ResolutionContext context)
        {
            var result = JsonSerializer.Serialize(source);
            return result;
        }
    }
}
