using Common.Dto.Views;
using Common.Models.States;

namespace UI.Extensions
{
    public static class LastMessagesListViewDtoExtension
    {
        /// <summary>
        /// Получает данные о сообщении для раздела "Сообщения", колонка в DataGrid "Автор"
        /// </summary>
        public static MessageData GetMessageInfoData(this LastMessagesListViewDto msg, CurrentState currentState)
        {
            var result = new MessageData();

            if (msg.Sender == null || msg.Recipient == null || currentState.Account == null)
                throw new Exception("Не найден отправитель или получатель сообщения!");

            result.Account = msg.Sender.Id == currentState.Account.Id ? msg.Recipient : msg.Sender;

            // Я получатель
            if (msg.Sender.Id != currentState.Account.Id)
            {
                result.Icon = "line_end_arrow_notch";

                if (msg.ReadDate == null)
                {
                    //result.IconStyle = IconStyle.Danger;
                    result.IcontTitle = "Не прочитано";
                }
                else
                {
                    //result.IconStyle = IconStyle.Success;
                    result.IcontTitle = $"Прочитано {msg.ReadDate.Value.ToMyString()}";
                }
            }
            // Я отправитель
            else
            {
                if (msg.ReadDate == null)
                {
                    result.Icon = "check";
                    //result.IconStyle = IconStyle.Danger;
                    result.IcontTitle = "Не прочитано";
                }
                else
                {
                    result.Icon = "done_all";
                    //result.IconStyle = IconStyle.Success;
                    result.IcontTitle = $"Прочитано {msg.ReadDate.Value.ToMyString()}";
                }
            }
            return result;
        }
    }

    public class MessageData
    {
        public AccountsViewDto Account { get; set; } = null!;
        public string Icon { get; set; } = null!;
        //public IconStyle IconStyle { get; set; }
        public string IcontTitle { get; set; } = null!;
    }
}
