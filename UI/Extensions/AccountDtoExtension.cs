using Common;
using Common.Dto.Views;
using Common.Enums;
using System.Text;

namespace UI.Extensions
{
    public static class AccountDtoExtension
    {
        /// <summary>
        /// Вывод всех пользователей учётки: пол + возраст (М45 Ж46)
        /// </summary>
        /// <param name="finish">HTML, добавляемый в конец строки</param>
        /// <returns>"М45 Ж46"</returns>
        public static string ToGendersString(this AccountsViewDto account, string? finish = null)
        {
            StringBuilder? genders = new StringBuilder(20);

            if (account.Users!.Count > 0)
            {
                genders.Append(account.Users
                    .OrderBy(o => o.Gender)
                    .Select(s =>
                    {
                        var age = DateTime.Today.Year - s.BirthDate.Year;
                        if (s.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;
                        return $"{StaticData.Genders[s.Gender].ShortName}{age}";
                    })
                    .Aggregate((a, b) => a + " " + b));
                genders.Append(finish);
            }

            return genders.ToString();
        }


        /// <summary>
        /// Конвертация страны и региона в текст
        /// </summary>
        /// <param name="finish">HTML, добавляемый в конец строки</param>
        /// <returns>"Россия, Москва"</returns>
        public static string ToRegionString(this AccountsViewDto account) =>
            $"{account.Country!.Name}, {account.Country.Region.Name}";


        /// <summary>
        /// Конвертация аватара учётки
        /// </summary>
        /// <param name="size">Размер аватара: "EnumImageSize.s64x64"</param>
        /// <returns>Путь к аватару</returns>
        public static string ToAvatarUri(this AccountsViewDto account, EnumImageSize size)
        {
            var photo = account.Avatar;
            return photo != null ? $"/images/AccountsPhotos/{account.Id}/{photo.Guid}/{size}.jpg" : $"/images/AccountsPhotos/no-avatar/{size}.jpg";
        }
    }
}
