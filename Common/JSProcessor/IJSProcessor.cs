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

        Task Redirect(string url);

        Task ScrollDivToBottom(string tagId);

        Task UpdateOnlineAccountsClient(HashSet<string> ConnectedAccounts);
    }
}
