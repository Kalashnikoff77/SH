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
        [Inject] IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> _repoGetSchedules { get; set; } = null!;


        public SignalRHub(
            IRepository<VisitsForAccountsUpdateRequestDto, ResponseDtoBase> repoUpdateVisits,
            IRepository<GetSchedulesRequestDto, GetSchedulesResponseDto> repoGetSchedules,
            Accounts connectedAccounts, IConfiguration configuration, ILogger<SignalRHub> logger)
        {
            _repoGetSchedules = repoGetSchedules;
            _repoUpdateVisits = repoUpdateVisits;

            _configuration = configuration;
            _logger = logger;
            Accounts = connectedAccounts;
        }


        // Глобальный обработчик всех поступающих запросов
        [Authorize]
        public async Task GlobalHandler(SignalGlobalRequest request)
        {
            // Изменения в расписании мероприятия (пока только добавлено одно сообщение в обсуждение)
            if (request.OnScheduleChanged != null)
                await OnScheduleChanged(request.OnScheduleChanged);

            // Пользователь изменил свой аватар
            if (request.OnAvatarChanged != null)
                await OnAvatarChanged(request.OnAvatarChanged);
        }


        /// <summary>
        /// Изменение в расписании мероприятия
        /// </summary>
        async Task OnScheduleChanged(OnScheduleChanged request)
        {
            var apiResponse = await _repoGetSchedules.HttpPostAsync(new GetSchedulesRequestDto { ScheduleId = request.ScheduleId });
            if (apiResponse.Response.Event != null)
            {
                var response = new OnScheduleChangedResponse { ScheduleForEventViewDto = apiResponse.Response.Event };
                await Clients.All.SendAsync(response.EnumSignalRHandlersClient.ToString(), response);
            }
        }

        /// <summary>
        /// Пользователь изменил свой аватар, уведомляем всех об этом
        /// </summary>
        async Task OnAvatarChanged(OnAvatarChanged request)
        {
            var response = new OnAvatarChangedResponse { NewAvatar = request.NewAvatar };
            await Clients.All.SendAsync(response.EnumSignalRHandlersClient.ToString(), response);
        }
    }
}
