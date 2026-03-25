using System.Text.Json;
namespace MTKPM_FE.Models
{
    public static class SessionExtensions
    {
        // Phương thức để "set" (lưu) một đối tượng vào Session
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Phương thức để "get" (lấy) một đối tượng từ Session
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}


