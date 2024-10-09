using Common.Dto.Views;
using MudBlazor;

namespace UI.Components.Shared.Dialogs
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

            var dialogParams = new DialogParameters<AccountCardDialog.AccountCardDialog>
            {
                { x => x.Account, account }
            };
            return _dialog.ShowAsync<AccountCardDialog.AccountCardDialog>(account.Name, dialogParams, dialogOptions);
        }

        /// <summary>
        /// Карточка мероприятия
        /// </summary>
        public Task PublicCardDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<PublicCardDialog.PublicCardDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return _dialog.ShowAsync<PublicCardDialog.PublicCardDialog>(schedule.Event?.Name, dialogParams, dialogOptions);
        }


        /// <summary>
        /// Регистрация на мероприятие
        /// </summary>
        public Task PublicRegistrationDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<PublicRegistrationDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return _dialog.ShowAsync<PublicRegistrationDialog>($"Подтверждение регистрации", dialogParams, dialogOptions);
        }

        /// <summary>
        /// Отмена регистрации на мероприятие
        /// </summary>
        public Task PublicCancelRegistrationDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<PublicRegistrationDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            return _dialog.ShowAsync<PublicCancelRegistrationDialog>($"Отмена регистрации", dialogParams, dialogOptions);
        }

    }
}
