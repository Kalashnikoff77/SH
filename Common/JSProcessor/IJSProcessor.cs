namespace Common.JSProcessor
{
    public interface IJSProcessor
    {
        /// <summary>
        /// Затухание и появление числа (напр.: изменилось кол-во непрочитанных сообщений в чате)
        /// </summary>
        /// <param name="tagClass">Класс тега</param>
        /// <param name="number">Выводимое число</param>
        /// <param name="isShowZero">Показывать ли ноль</param>
        Task ChangeNumberFadeInOut(string tagClass, int? number, bool IsShowZero = false);

        /// <summary>
        /// Затухание и появление числа + цвета кнопки (напр.: изменилось кол-во непрочитанных сообщений в чате)
        /// </summary>
        /// <param name="tagClass">Класс тега</param>
        /// <param name="number">Выводимое число</param>
        /// <param name="isShowZero">Показывать ли ноль</param>
        Task ChangeNumberInButtonsFadeInOut(string tagClass, int? number);

        Task Redirect(string url);

        Task ScrollDivToBottom(string divId);
        Task ScrollToElement(string elementId);

        Task UpdateOnlineAccountsClient(HashSet<string> ConnectedAccounts);
    }
}
