using Common.Enums;
using Common.Models;
using Common.Models.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
    public partial class SignalRHub
    {
        // Триггер для обновления кол-ва подписавшихся или зареганых на определённое мероприятие
        [Authorize]
        public async Task UpdateEventRegisterServer(UpdateEventRegisterModel model)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(UpdateEventRegisterServer));
            _logger.LogInformation("МЕТОД: {0}({@model})", nameof(UpdateEventRegisterServer), model);

            //var apiResponse = await _repoGetEventsSRD.HttpPostAsync(new GetEventsSRDModel());
            //model.Events = apiResponse.Response.Events;

            _logger.LogInformation("Clients.All({1}, {@model})", EnumSignalRHandlers.UpdateEventRegisterClient, model);
            await Clients.All.SendAsync(nameof(EnumSignalRHandlers.UpdateEventRegisterClient), model);
        }


        // Триггер всем пользователям о добавлении нового комментария в мероприятии
        [Authorize]
        public async Task NewEventDiscussionAddedServer(NewEventDiscussionAddedModel model)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(NewEventDiscussionAddedServer));
            _logger.LogInformation("МЕТОД: {0}({@1})", nameof(NewEventDiscussionAddedServer), model);

            _logger.LogInformation("Clients.All.{0}({1}, {2})", EnumSignalRHandlers.NewEventDiscussionAddedClient, "All", model);
            await Clients.All.SendAsync(nameof(EnumSignalRHandlers.NewEventDiscussionAddedClient), model);
        }
    }

}
