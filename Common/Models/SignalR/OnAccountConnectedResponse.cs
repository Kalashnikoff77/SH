using Common.Enums;
using Common.Models.States;
using Microsoft.JSInterop;

namespace Common.Models.SignalR
{
    public class OnAccountConnectedResponse : SignalRModelBase<OnAccountConnectedResponse>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.UpdateOnlineAccountsClient;

        public IEnumerable<string> ConnectedAccounts { get; set; } = null!;

        public override Func<OnAccountConnectedResponse, Task> Func(CurrentState currentState)
        {
            return async (response) =>
            {
                currentState.ConnectedAccounts = response.ConnectedAccounts.ToHashSet();
                await currentState.JS.InvokeVoidAsync(EnumSignalRHandlersClient.ToString(), response.ConnectedAccounts);
            };
        }
    }
}
