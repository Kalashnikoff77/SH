using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Views;
using Common.Mapping.Converters;

namespace Common.Mapping
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            //CreateMap<AccountsViewDto, UpdateAccountRequestDto>()
            //    .ForMember(to => to.Informing, from => from.ConvertUsing<InformingConverter, string?>(from => from.Informing))
            //    .ForMember(to => to.Password2, from => from.MapFrom(from => from.Password));
        }
    }
}
