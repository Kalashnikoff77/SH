using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Models;
using Common.Repository;
using SignalR.Models;

namespace SignalR
{
    public partial class SignalRHub
    {
        /// <summary>
        /// Список онлайн пользователей
        /// </summary>
        Accounts Accounts { get; set; } = null!;

        IConfiguration _configuration;
        ILogger<SignalRHub> _logger;

        IRepository<AccountVisitsUpdateModel, AccountVisitsUpdateRequestDto, ResponseDtoBase> _repoUpdateVisits { get; set; } = null!;
        IRepository<RelationsUpdateModel, RelationsUpdateRequestDto, RelationsUpdateResponseDto> _repoUpdateRelations { get; set; } = null!;
        IRepository<GetAccountsModel, GetAccountsRequestDto, GetAccountsResponseDto> _repoGetAccounts { get; set; } = null!;
        IRepository<AddNotificationModel, AddNotificationRequestDto, ResponseDtoBase> _repoAddNotification { get; set; } = null!;
        IRepository<GetEventsModel, GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;
        IRepository<GetEventsSRDModel, GetEventsSRDRequestDto, GetEventsResponseDto> _repoGetEventsSRD { get; set; } = null!;

        public SignalRHub(
            IRepository<AccountVisitsUpdateModel, AccountVisitsUpdateRequestDto, ResponseDtoBase> repoUpdateVisits,
            IRepository<RelationsUpdateModel, RelationsUpdateRequestDto, RelationsUpdateResponseDto> repoUpdateRelations,
            IRepository<GetAccountsModel, GetAccountsRequestDto, GetAccountsResponseDto> repoGetAccounts,
            IRepository<AddNotificationModel, AddNotificationRequestDto, ResponseDtoBase> repoAddNotification,
            IRepository<GetEventsModel, GetEventsRequestDto, GetEventsResponseDto> repoGetEvents,
            IRepository<GetEventsSRDModel, GetEventsSRDRequestDto, GetEventsResponseDto> repoGetEventsSRD,
            Accounts connectedAccounts, IConfiguration configuration, ILogger<SignalRHub> logger)
        {
            _repoGetAccounts = repoGetAccounts;
            _repoUpdateVisits = repoUpdateVisits;
            _repoUpdateRelations = repoUpdateRelations;
            _repoAddNotification = repoAddNotification;
            _repoGetEvents = repoGetEvents;
            _repoGetEventsSRD = repoGetEventsSRD;

            _configuration = configuration;
            _logger = logger;
            Accounts = connectedAccounts;
        }
    }
}
