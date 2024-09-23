using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Models.SignalR;
using Common.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
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

        [Inject] IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> _repoUpdateVisits { get; set; } = null!;
        [Inject] IRepository<GetEventOneRequestDto, GetEventOneResponseDto> _repoGetEvent { get; set; } = null!;
        [Inject] IRepository<AddDiscussionsForEventsRequestDto, AddDiscussionsForEventsResponseDto> _repoAddDiscussion { get; set; } = null!;


        public SignalRHub(
            IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> repoUpdateVisits,
            IRepository<GetEventOneRequestDto, GetEventOneResponseDto> repoGetEvent,
            IRepository<AddDiscussionsForEventsRequestDto, AddDiscussionsForEventsResponseDto> _repoAddDiscussion,
            Accounts connectedAccounts, IConfiguration configuration, ILogger<SignalRHub> logger)
        {
            _repoGetEvent = repoGetEvent;
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
                await OnEventDiscussionAdded(request.OnEventDiscussionAdded);
        }


        /// <summary>
        /// Добавлено обсуждение в мероприятие
        /// </summary>
        async Task OnEventDiscussionAdded(OnEventDiscussionAdded request)
        {
            if (GetAccountDetails(out AccountDetails accountDetails, Context.UserIdentifier))
            {
                var apiResponse = await _repoGetEvent.HttpPostAsync(new GetEventOneRequestDto { ScheduleId = request.ScheduleId });
                if (apiResponse.Response.Event != null)
                {
                    var response = new OnEventDiscussionAddedResponse { ScheduleForEventViewDto = apiResponse.Response.Event };
                    await Clients.All.SendAsync(response.EnumSignalRHandlersClient.ToString(), response);
                }
            }
        }

    }
}
