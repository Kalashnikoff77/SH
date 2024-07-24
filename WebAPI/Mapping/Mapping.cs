using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Views;
using DataContext.Entities;
using DataContext.Entities.Views;
using WebAPI.Mapping.Converters;

namespace WebAPI.Mapping
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AccountsViewEntity, AccountsViewDto>()
                .ForMember(to => to.Users, from => from.ConvertUsing<JsonToClassConverter<List<UsersDto>>, string?>(from => from.Users))
                .ForMember(to => to.Country, from => from.ConvertUsing<JsonToClassConverter<CountriesDto>, string?>(from => from.Country))
                .ForMember(to => to.Avatar, from => from.ConvertUsing<JsonToClassConverter<AccountsPhotosDto>, string?>(from => from.Avatar))
                .ForMember(to => to.Photos, from => from.ConvertUsing<JsonToClassConverter<List<AccountsPhotosDto>>, string?>(from => from.Photos))
                .ForMember(to => to.Events, from => from.ConvertUsing<JsonToClassConverter<List<EventsViewDto>>, string?>(from => from.Events))
                .ForMember(to => to.Hobbies, from => from.ConvertUsing<JsonToClassConverter<List<AccountsHobbiesDto>>, string?>(from => from.Hobbies))
                .ForMember(to => to.Relations, from => from.ConvertUsing<JsonToClassConverter<List<AccountsRelationsDto>>, string?>(from => from.Relations));

            CreateMap<CountriesViewEntity, CountriesViewDto>()
                .ForMember(to => to.Regions, from => from.ConvertUsing<JsonToClassConverter<List<RegionsDto>>, string?>(from => from.Regions));

            CreateMap<EventsViewEntity, EventsViewDto>()
                .ForMember(to => to.Admin, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Admin))
                .ForMember(to => to.Country, from => from.ConvertUsing<JsonToClassConverter<CountriesDto>, string?>(from => from.Country))
                .ForMember(to => to.Avatar, from => from.ConvertUsing<JsonToClassConverter<EventsPhotosDto>, string?>(from => from.Avatar))
                .ForMember(to => to.Photos, from => from.ConvertUsing<JsonToClassConverter<List<EventsPhotosDto>>, string?>(from => from.Photos));

            CreateMap<EventsDiscussionsViewEntity, EventsDiscussionsViewDto>()
                .ForMember(to => to.Sender, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Sender))
                .ForMember(to => to.Recipient, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Recipient));

            CreateMap<WishListViewEntity, WishListViewDto>()
                .ForMember(to => to.Users, from => from.ConvertUsing<JsonToClassConverter<List<UsersDto>>, string?>(from => from.Users))
                .ForMember(to => to.Country, from => from.ConvertUsing<JsonToClassConverter<CountriesDto>, string?>(from => from.Country))
                .ForMember(to => to.Avatar, from => from.ConvertUsing<JsonToClassConverter<AccountsPhotosDto>, string?>(from => from.Avatar))
                .ForMember(to => to.Relations, from => from.ConvertUsing<JsonToClassConverter<List<AccountsRelationsDto>>, string?>(from => from.Relations));

            CreateMap<LastMessagesListViewEntity, LastMessagesListViewDto>()
                .ForMember(to => to.Sender, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Sender))
                .ForMember(to => to.Recipient, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Recipient));

            CreateMap<NotificationsViewEntity, NotificationsViewDto>()
                .ForMember(to => to.Sender, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Sender));

            CreateMap<AccountsPhotosViewEntity, AccountsPhotosViewDto>()
                .ForMember(to => to.Account, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Account));

            CreateMap<MessagesEntity, MessagesDto>();

            // Регистрация аккаунта
            CreateMap<AccountRegisterRequestDto, AccountsEntity>()
                .ForMember(to => to.RegionId, from => from.MapFrom(from => from.Country.Region.Id))
                .ForMember(to => to.Informing, from => from.ConvertUsing<InformingToStringConverter, Informing>(from => from.Informing));
            CreateMap<UsersDto, UsersEntity>();


            //CreateMap<NotificationAdoEntity, NotificationAdoDto>();

            //CreateMap<EventsViewAdoEntity, EventsViewAdoDto>()





            // СТАРЫЙ ВАРИАНТ, УДАЛИТЬ

            //CreateMap<AccountsEntity, AccountsDto>()
            //.ForMember(to => to.Password, from => from.MapFrom(from => from.AccountPassword.Password))
            //.ForMember(to => to.Informing, from => from.ConvertUsing<StringToInformingConverter, string>(from => from.Informing));

            //CreateMap<AccountPhotoEntity, AccountsPhotosDto>().ReverseMap();
            //CreateMap<AccountsBlogsEntity, AccountsBlogsDto>().ReverseMap();
            //CreateMap<AccountsWishListsEntity, AccountsWishListsDto>().ReverseMap();

            //CreateMap<NotificationEntity, NotificationsDto>().ReverseMap();
            //CreateMap<MessageEntity, MessagesDto>().ReverseMap();
            //CreateMap<RelationEntity, RelationsDto>().ReverseMap();
            //CreateMap<HobbyEntity, HobbiesDto>().ReverseMap();

            //CreateMap<UsersEntity, UsersDto>().ReverseMap();
            //CreateMap<CountriesEntity, CountriesDto>().ReverseMap();

            //CreateMap<RegionEntity, RegionsDto>().ReverseMap();

            //CreateMap<EventEntity, EventsDto>().ReverseMap();
            //CreateMap<EventPhotoEntity, EventsPhotosDto>().ReverseMap();
        }
    }
}
