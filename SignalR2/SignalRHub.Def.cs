using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Repository;
using SignalR2.Models;

namespace SignalR2
{
    public partial class SignalRHub
    {
        /// <summary>
        /// Список онлайн пользователей
        /// </summary>
        Accounts Accounts { get; set; } = null!;

        IConfiguration _configuration;
        ILogger<SignalRHub> _logger;

        IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> _repoUpdateVisits { get; set; } = null!;
        IRepository<RelationsUpdateRequestDto, RelationsUpdateResponseDto> _repoUpdateRelations { get; set; } = null!;
        IRepository<GetAccountsRequestDto, GetAccountsResponseDto> _repoGetAccounts { get; set; } = null!;
        IRepository<AddNotificationRequestDto, ResponseDtoBase> _repoAddNotification { get; set; } = null!;
        IRepository<GetEventsRequestDto, GetEventsResponseDto> _repoGetEvents { get; set; } = null!;

        public SignalRHub(
            IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> repoUpdateVisits,
            IRepository<RelationsUpdateRequestDto, RelationsUpdateResponseDto> repoUpdateRelations,
            IRepository<GetAccountsRequestDto, GetAccountsResponseDto> repoGetAccounts,
            IRepository<AddNotificationRequestDto, ResponseDtoBase> repoAddNotification,
            IRepository<GetEventsRequestDto, GetEventsResponseDto> repoGetEvents,
            Accounts connectedAccounts, IConfiguration configuration, ILogger<SignalRHub> logger)
        {
            _repoGetAccounts = repoGetAccounts;
            _repoUpdateVisits = repoUpdateVisits;
            _repoUpdateRelations = repoUpdateRelations;
            _repoAddNotification = repoAddNotification;
            _repoGetEvents = repoGetEvents;

            _configuration = configuration;
            _logger = logger;
            Accounts = connectedAccounts;
        }
    }
}
