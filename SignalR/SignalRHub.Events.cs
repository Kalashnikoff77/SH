using Common.Enums;
using Common.Models.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
    public partial class SignalRHub
    {
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
