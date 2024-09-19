using Common.Dto.Requests;
using Common.Enums;
using Common.Models.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalR2.Models;

namespace SignalR2
{
    public partial class SignalRHub
    {
        // Обновление связей между пользователями, триггерим обоих пользователей
        [Authorize]
        public async Task UpdateRelationsServer(UpdateRelationsModel model)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(UpdateRelationsServer));
            _logger.LogInformation("МЕТОД: {0}({@1})", nameof(UpdateRelationsServer), model);

            if (GetAccountDetails(out AccountDetails accountDetails, Context.UserIdentifier))
            {
                // Получим подробные данные о текущем пользователе
                var apiResponse = await _repoGetAccounts.HttpPostAsync(new GetAccountsRequestDto { Id = accountDetails.Id, IsRelationsIncluded = true, Take = 1 });
                var currentUser = apiResponse.Response.Accounts.FirstOrDefault();

                if (currentUser != null)
                {
                    currentUser.Token = accountDetails.Token;

                    // Добавим или удалим запись в таблице Relations
                    var requestRelation = new RelationsUpdateRequestDto
                    {
                        RecipientId = model.RecipientId,
                        EnumRelation = model.EnumRelation,
                        Token = currentUser.Token
                    };
                    var apiUpdateRelationResponse = await _repoUpdateRelations.HttpPostAsync(requestRelation);

                    // Добавляем инфу о запросе дружбы в Notifications в БД
                    var requestNotification = new AddNotificationRequestDto
                    {
                        RecipientId = model.RecipientId,
                        //EnumRelation = model.EnumRelation,
                        Token = currentUser.Token
                    };

                    if (model.EnumRelation == EnumRelations.Friend && model.IsAdding == true && model.IsConfirmed == false)
                        requestNotification.Text = $" хотят с Вами подружиться.";
                    else if (model.EnumRelation == EnumRelations.Friend && model.IsAdding == false && model.IsConfirmed == true && model.IsRemoving == false)
                        requestNotification.Text = $" приняли Ваше предложение дружбы.";
                    else if (model.EnumRelation == EnumRelations.Friend && model.IsRemoving == true)
                        requestNotification.Text = $" прекратили с Вами дружбу.";

                    if (model.EnumRelation == EnumRelations.Blocked)
                    {
                        if (apiUpdateRelationResponse.Response.IsRelationAdded)
                            requestNotification.Text = $" Вас заблокировали.";
                        else
                            requestNotification.Text = $" Вас разблокировали.";
                    }

                    if (model.EnumRelation == EnumRelations.Subscriber)
                    {
                        if (apiUpdateRelationResponse.Response.IsRelationAdded)
                            requestNotification.Text = $" подписались на Вас.";
                        else
                            requestNotification.Text = $" отписались от Вас.";
                    }

                    var apiAddNotificationResponse = await _repoAddNotification.HttpPostAsync(requestNotification);

                    _logger.LogInformation("Clients.User.{0}({1})", EnumSignalRHandlers.UpdateNotificationsCountClient, model.RecipientId);
                    await Clients
                        .User(model.RecipientId.ToString())
                        .SendAsync(nameof(EnumSignalRHandlers.UpdateNotificationsCountClient), model);
                }
            }
        }


        // Триггер для обновления связей между пользователями, триггерим обоих
        [Authorize]
        public async Task GetRelationsServer(int recipientId)
        {
            var loggerScope = _logger.BeginScope("{@CurrentMethod}", nameof(GetRelationsServer));
            _logger.LogInformation("МЕТОД: {0}({1})", nameof(GetRelationsServer), recipientId);

            if (GetAccountDetails(out AccountDetails senderDetails, Context.UserIdentifier))
            {
                var apiAccountResponse = await _repoGetAccounts.HttpPostAsync(new GetAccountsRequestDto
                {
                    Id = recipientId,
                    IsRelationsIncluded = true,
                    Take = 1
                });
                var recipientAccount = apiAccountResponse.Response.Accounts.FirstOrDefault();

                if (recipientAccount != null)
                {
                    var updateRelationsTriggerModel = new GetRelationsModel
                    {
                        SenderId = senderDetails.Id,
                        RecipientId = recipientId,
                        Relations = recipientAccount.Relations
                    };
                    _logger.LogInformation("Clients.Users.{0}({1}, {2}), {@3}", EnumSignalRHandlers.GetRelationsClient, Context.UserIdentifier, recipientId, updateRelationsTriggerModel);
                    await Clients
                        .Users([Context.UserIdentifier!, recipientId.ToString()])
                        .SendAsync(nameof(EnumSignalRHandlers.GetRelationsClient), updateRelationsTriggerModel);
                }
            }
        }
    }
}
