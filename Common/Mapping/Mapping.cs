using AutoMapper;
using Common.Dto.Requests;
using Common.Dto.Views;
using Common.Mapping.Converters;
using Common.Models;

namespace Common.Mapping
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<LoginModel, LoginRequestDto>();
            CreateMap<UpdatePhotoModel, UpdatePhotoRequestDto>();
            CreateMap<UploadPhotoModel, UploadPhotoRequestDto>();
            CreateMap<AccountRegisterModel, AccountRegisterRequestDto>();
            CreateMap<AccountReloadModel, AccountReloadRequestDto>();
            CreateMap<AccountUpdateModel, AccountUpdateRequestDto>();
            CreateMap<RelationsUpdateModel, RelationsUpdateRequestDto>();
            CreateMap<AccountVisitsUpdateModel, AccountVisitsUpdateRequestDto>();
            CreateMap<GetMessagesModel, GetMessagesRequestDto>();
            CreateMap<GetMessagesCountModel, GetMessagesCountRequestDto>();
            CreateMap<GetLastMessagesListModel, GetLastMessagesListRequestDto>();
            CreateMap<AddMessageModel, AddMessageRequestDto>();
            CreateMap<AddNotificationModel, AddNotificationRequestDto>();
            CreateMap<GetNotificationsCountModel, GetNotificationsCountRequestDto>();
            CreateMap<GetNotificationsModel, GetNotificationsRequestDto>();
            CreateMap<GetAccountPhotosModel, GetAccountPhotosRequestDto>();
            CreateMap<GetAccountsModel, GetAccountsRequestDto>();
            CreateMap<GetAccountModel, GetAccountsRequestDto>();
            CreateMap<GetCountriesModel, GetCountriesRequestDto>();
            CreateMap<GetWishListModel, GetWishListRequestDto>();
            CreateMap<GetEventsModel, GetEventsRequestDto>();
            CreateMap<GetEventOneModel, GetEventOneRequestDto>();
            CreateMap<GetEventsSRDModel, GetEventsSRDRequestDto>();
            CreateMap<GetEventDiscussionsModel, GetEventDiscussionsRequestDto>();
            CreateMap<AddEventDiscussionModel, AddEventDiscussionRequestDto>();
            CreateMap<UpdateEventSubscriptionModel, UpdateEventSubscriptionRequestDto>();
            CreateMap<GetAccountRelationsModel, GetAccountRelationsRequestDto>();

            // В компоненте редактирования учётной записи
            CreateMap<AccountsViewDto, AccountUpdateModel>()
                .ForMember(to => to.Informing, from => from.ConvertUsing<InformingConverter, string?>(from => from.Informing));
        }
    }
}
