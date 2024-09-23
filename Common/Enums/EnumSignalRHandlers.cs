namespace Common.Enums
{
    public enum EnumSignalRHandlers
    {
        /// <summary>
        /// При добавлении обсуждения в мероприятие
        /// </summary>
        OnEventDiscussionAddedClient,

        // Требуется проверить, используются ли эти перечисления
        NewMessageAddedServer,
        NewMessageAddedClient,

        NewEventDiscussionAddedServer,
        NewEventDiscussionAddedClient,

        NewNotificationAddedServer,
        NewNotificationAddedClient,

        UpdateMessagesCountServer,
        UpdateMessagesCountClient,

        UpdateNotificationsCountServer,
        UpdateNotificationsCountClient,

        UpdateOnlineAccountsClient,

        AvatarChangedServer,
        AvatarChangedClient,

        UpdateRelationsServer,
        GetRelationsClient,
        GetRelationsServer
    }
}
