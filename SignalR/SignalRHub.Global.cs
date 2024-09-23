using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Models.SignalR;
using Common.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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

        IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> _repoUpdateVisits { get; set; } = null!;

        public SignalRHub(IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> repoUpdateVisits,
            Accounts connectedAccounts, IConfiguration configuration, ILogger<SignalRHub> logger)
        {
            _repoUpdateVisits = repoUpdateVisits;

            _configuration = configuration;
            _logger = logger;
            Accounts = connectedAccounts;
        }


        // Глобальный обработчик всех поступающих запросов
        [Authorize]
        public async Task GlobalHandler(SignalGlobalRequest request)
        {
            if (request.OnEventDiscussionAdded != null)
                await OnEventDiscussionAdded(request);
        }


        async Task OnEventDiscussionAdded(SignalGlobalRequest request)
        {
            var response = new OnEventDiscussionAddedResponse { Id = request.OnEventDiscussionAdded.EventId };
            await Clients.All.SendAsync("OnEventDiscussionAddedClient", response);
        }
    }
}
