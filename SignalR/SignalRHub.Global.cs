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
        [Inject] IRepository<GetScheduleOneRequestDto, GetScheduleOneResponseDto> _repoGetEvent { get; set; } = null!;


        public SignalRHub(
            IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> repoUpdateVisits,
            IRepository<GetScheduleOneRequestDto, GetScheduleOneResponseDto> repoGetEvent,
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
            if (request.OnScheduleChanged != null)
                await OnEventDiscussionAdded(request.OnScheduleChanged);
        }


        /// <summary>
        /// Добавлено обсуждение в мероприятие
        /// </summary>
        async Task OnEventDiscussionAdded(OnScheduleChanged request)
        {
            var apiResponse = await _repoGetEvent.HttpPostAsync(new GetScheduleOneRequestDto { ScheduleId = request.ScheduleId });
            if (apiResponse.Response.Event != null)
            {
                var response = new OnScheduleChangedResponse { ScheduleForEventViewDto = apiResponse.Response.Event };
                await Clients.All.SendAsync(response.EnumSignalRHandlersClient.ToString(), response);
            }
        }
    }
}
