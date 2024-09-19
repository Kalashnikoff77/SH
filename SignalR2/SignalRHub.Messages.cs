using Common.Enums;
using Common.Models.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalR2
{
    public partial class SignalRHub
    {
        // Триггер обоим собеседникам обновить иконку кол-ва непрочитанных сообщений
        [Authorize]
        public async Task UpdateMessagesCountServer(int recipientId)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(UpdateMessagesCountServer));
            _logger.LogInformation("МЕТОД: {0}({1})", nameof(UpdateMessagesCountServer), recipientId);

            _logger.LogInformation("Clients.Users.{0}({1}, {2})", EnumSignalRHandlers.UpdateMessagesCountClient, Context.UserIdentifier, recipientId);

            var model = new UpdateMessagesCountModel();
            await Clients
                .Users([Context.UserIdentifier!, recipientId.ToString()])
                .SendAsync(nameof(EnumSignalRHandlers.UpdateMessagesCountClient), model);
        }

        // Триггер обоим собеседникам о добавлении сообщения в их чате
        [Authorize]
        public async Task NewMessageAddedServer(NewMessageAddedModel model)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(NewMessageAddedServer));
            _logger.LogInformation("МЕТОД: {0}({@1})", nameof(NewMessageAddedServer), model);

            // Триггер обоим собеседникам (обновить сообщения в главном окне сообщений)
            _logger.LogInformation("Clients.Users.{0}({1}, {2}), {@3}", EnumSignalRHandlers.NewMessageAddedClient, Context.UserIdentifier, model.RecipientId, model);
            await Clients
                .Users([Context.UserIdentifier!, model.RecipientId.ToString()])
                .SendAsync(nameof(EnumSignalRHandlers.NewMessageAddedClient), model);

            // Триггер обоим собседеникам (обновить иконку сообщений)
            await UpdateMessagesCountServer(model.RecipientId);
        }
    }
}
