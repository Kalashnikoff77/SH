using Microsoft.AspNetCore.Components;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Common.Extensions
{
    public static class GlobalExtension
    {
        /// <summary>
        /// Глубокое копирование через JSON
        /// </summary>
        public static T? DeepCopy<T>(this object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            var result = JsonSerializer.Deserialize<T>(json)!;
            return result;
        }


        /// <summary>
        /// Замена \n\r на <br>
        /// </summary>
        public static MarkupString ReplaceNewLineWithBR(this string text) =>
            new MarkupString(Regex.Replace(text, @"\r\n?|\n", "<br />"));


        /// <summary>
        /// Удаление пустых линий
        /// </summary>
        public static string RemoveEmptyLines(this string text) =>
            Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
    }
}
