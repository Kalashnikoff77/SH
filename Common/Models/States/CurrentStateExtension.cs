using Common.Enums;
using Common.Models.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Common.Models.States
{
    public static class CurrentStateExtension
    {
        /// <summary>
        /// Обработчик ответа внутри модели
        /// </summary>
        public static IDisposable? SignalRClient<T>(this IDisposable? clientHandler, CurrentState currentState)
            where T : SignalRModelBase<T>, new()
        {
            T TModel = new T();

            if (clientHandler == null && currentState.SignalR != null)
                clientHandler = currentState.SignalR.On(TModel.EnumSignalRHandlersClient.ToString(), TModel.Func(currentState));

            return clientHandler;
        }

        /// <summary>
        /// Обработчик ответа в параметрах
        /// </summary>
        public static IDisposable? SignalRClient<T>(this IDisposable? clientHandler, CurrentState currentState, Func<T, Task> func)
            where T : SignalRModelBase<T>, new()
        {
            T TModel = new T();

            if (clientHandler == null && currentState.SignalR != null)
                clientHandler = currentState.SignalR.On<T>(TModel.EnumSignalRHandlersClient.ToString(), func.Invoke);

            return clientHandler;
        }

        /// <summary>
        /// Отправка запроса на сервер SignalR
        /// </summary>
        public static Task SignalRServerAsync(this CurrentState currentState, EnumSignalRHandlers enumSignalRHandlers, object? arg1 = null)
        {
            if (currentState.SignalR != null)
            {
                if (arg1 == null)
                    return currentState.SignalR.SendAsync(enumSignalRHandlers.ToString());
                else
                    return currentState.SignalR.SendAsync(enumSignalRHandlers.ToString(), arg1);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
