﻿using Common.Enums;
using Common.Models.States;
using Microsoft.JSInterop;

namespace Common.Models.SignalR2
{
    public abstract class SignalRModelBase<T>
    {
        /// <summary>
        /// Название метода, вызывающегося в JS
        /// </summary>
        public abstract EnumSignalRHandlers EnumSignalRHandlersClient { get; }

        /// <summary>
        /// По умолчанию сразу вызывается метод в JS. Иначе класс можно переопределить
        /// </summary>
        public virtual Func<T, Task> Func(CurrentState currentState) => async (response) =>
            await currentState.JS.InvokeVoidAsync(EnumSignalRHandlersClient.ToString(), response);
    }
}
