using System.Text.Json;

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
    }
}
