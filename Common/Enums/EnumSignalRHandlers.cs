namespace Common.Enums
{
    public enum EnumSignalRHandlers
    {
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
        GetRelationsServer,

        UpdateEventRegisterServer,
        UpdateEventRegisterClient
    }
}
