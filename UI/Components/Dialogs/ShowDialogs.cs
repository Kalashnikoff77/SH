using Common.Dto.Views;
using MudBlazor;

namespace UI.Components.Dialogs
{
    public class ShowDialogs
    {
        IDialogService _dialog { get; set; } = null!;

        public ShowDialogs(IDialogService dialog) => _dialog = dialog;

        /// <summary>
        /// Карточка аккаунта
        /// </summary>
        public Task AccountCardDialogAsync(AccountsViewDto account)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<AccountCardDialog>
            {
                { x => x.Account, account }
            };
            return _dialog.ShowAsync<AccountCardDialog>(account.Name, dialogParams, dialogOptions);
        }

        /// <summary>
        /// Карточка мероприятия
        /// </summary>
        public Task EventCardDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<EventCardDialog.EventCardDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return _dialog.ShowAsync<EventCardDialog.EventCardDialog>(schedule.Event?.Name, dialogParams, dialogOptions);
        }


        /// <summary>
        /// Подвтерждение регистрация на клубное мероприятие
        /// </summary>
        public Task RegistrationForEventDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<RegisterForEventDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return _dialog.ShowAsync<RegisterForEventDialog>($"Подтверждение регистрации", dialogParams, dialogOptions);
        }

        /// <summary>
        /// Отмена регистрации на клубное мероприятие
        /// </summary>
        public Task CancelRegistrationForEventDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<RegisterForEventDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return _dialog.ShowAsync<UnregisterForEventDialog>($"Отмена регистрации", dialogParams, dialogOptions);
        }

    }
}
