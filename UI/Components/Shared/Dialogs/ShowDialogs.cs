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
        /// Регистрация на мероприятие
        /// </summary>
        public async Task EventRegistrationDialogAsync(SchedulesForEventsViewDto schedule)
        {
            DialogOptions dialogOptions = new() { CloseOnEscapeKey = true, CloseButton = true };

            var dialogParams = new DialogParameters<EventRegistrationDialog>
            {
                { x => x.ScheduleForEventView, schedule }
            };
            var resultDialog = await _dialog.ShowAsync<EventRegistrationDialog>(schedule.Event?.Name, dialogParams, dialogOptions);

        }


    }
}
