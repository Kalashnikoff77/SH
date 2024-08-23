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
                .ForMember(to => to.Avatar, from => from.ConvertUsing<JsonToClassConverter<PhotosForAccountsDto>, string?>(from => from.Avatar))
                .ForMember(to => to.Photos, from => from.ConvertUsing<JsonToClassConverter<List<PhotosForAccountsDto>>, string?>(from => from.Photos))
                .ForMember(to => to.Events, from => from.ConvertUsing<JsonToClassConverter<List<EventsViewDto>>, string?>(from => from.Events))
                .ForMember(to => to.Hobbies, from => from.ConvertUsing<JsonToClassConverter<List<HobbiesForAccountsDto>>, string?>(from => from.Hobbies))
                .ForMember(to => to.Relations, from => from.ConvertUsing<JsonToClassConverter<List<RelationsForAccountsDto>>, string?>(from => from.Relations));

            CreateMap<CountriesViewEntity, CountriesViewDto>()
                .ForMember(to => to.Regions, from => from.ConvertUsing<JsonToClassConverter<List<RegionsDto>>, string?>(from => from.Regions));

            CreateMap<EventsViewEntity, EventsViewDto>()
                .ForMember(to => to.Admin, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Admin))
                .ForMember(to => to.Schedule, from => from.ConvertUsing<JsonToClassConverter<List<SchedulesForEventsDto>>, string?>(from => from.Schedule))
                .ForMember(to => to.Country, from => from.ConvertUsing<JsonToClassConverter<CountriesDto>, string?>(from => from.Country))
                .ForMember(to => to.Avatar, from => from.ConvertUsing<JsonToClassConverter<PhotosForEventsDto>, string?>(from => from.Avatar))
                .ForMember(to => to.Photos, from => from.ConvertUsing<JsonToClassConverter<List<PhotosForEventsDto>>, string?>(from => from.Photos));

            CreateMap<SchedulesForEventsViewEntity, SchedulesForEventsViewDto>()
                .ForMember(to => to.Event, from => from.ConvertUsing<JsonToClassConverter<EventsViewDto>, string?>(from => from.Event))
                .ForMember(to => to.Features, from => from.ConvertUsing<JsonToClassConverter<List<FeaturesDto>>, string?>(from => from.Features));

            CreateMap<DiscussionsForEventsViewEntity, DiscussionsForEventsViewDto>()
                .ForMember(to => to.Sender, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Sender))
                .ForMember(to => to.Recipient, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Recipient));

            CreateMap<WishListViewEntity, WishListViewDto>()
                .ForMember(to => to.Users, from => from.ConvertUsing<JsonToClassConverter<List<UsersDto>>, string?>(from => from.Users))
                .ForMember(to => to.Country, from => from.ConvertUsing<JsonToClassConverter<CountriesDto>, string?>(from => from.Country))
                .ForMember(to => to.Avatar, from => from.ConvertUsing<JsonToClassConverter<PhotosForAccountsDto>, string?>(from => from.Avatar))
                .ForMember(to => to.Relations, from => from.ConvertUsing<JsonToClassConverter<List<RelationsForAccountsDto>>, string?>(from => from.Relations));

            CreateMap<LastMessagesListViewEntity, LastMessagesListViewDto>()
                .ForMember(to => to.Sender, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Sender))
                .ForMember(to => to.Recipient, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Recipient));

            CreateMap<NotificationsViewEntity, NotificationsViewDto>()
                .ForMember(to => to.Sender, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Sender));

            CreateMap<PhotosForAccountsViewEntity, PhotosForAccountsViewDto>()
                .ForMember(to => to.Account, from => from.ConvertUsing<JsonToClassConverter<AccountsViewDto>, string?>(from => from.Account));

            CreateMap<MessagesEntity, MessagesDto>();

            CreateMap<FeaturesEntity, FeaturesDto>();

            // Регистрация аккаунта
            CreateMap<AccountRegisterRequestDto, AccountsEntity>()
                .ForMember(to => to.RegionId, from => from.MapFrom(from => from.Country.Region.Id))
                .ForMember(to => to.Informing, from => from.ConvertUsing<InformingToStringConverter, Informing>(from => from.Informing));
            CreateMap<UsersDto, UsersEntity>();
        }
    }
}
