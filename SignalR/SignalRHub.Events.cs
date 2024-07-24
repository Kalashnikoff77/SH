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
        public async Task UpdateEventSubscribeOrRegisterServer(UpdateEventSubscribeOrRegisterModel model)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(UpdateEventSubscribeOrRegisterServer));
            _logger.LogInformation("МЕТОД: {0}({@model})", nameof(UpdateEventSubscribeOrRegisterServer), model);

            var apiResponse = await _repoGetEventsSRD.HttpPostAsync(new GetEventsSRDModel());
            model.Events = apiResponse.Response.Events;

            _logger.LogInformation("Clients.All({1}, {@model})", EnumSignalRHandlers.UpdateEventSubscribeOrRegisterClient, model);
            await Clients.All.SendAsync(nameof(EnumSignalRHandlers.UpdateEventSubscribeOrRegisterClient), model);
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
