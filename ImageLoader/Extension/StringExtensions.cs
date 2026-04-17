using System.Text;

namespace ImageLoader.Extension;

public static class StringExtensions
{
    internal static string ToSanitizedKey(this string key)
    {
        return new string(key.ToCharArray()
            .Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            .ToArray());
    }

    internal static string Md5(this string input)
    {
        using var hashProvider = System.Security.Cryptography.MD5.Create();
        
        var bytes = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes)?.ToSanitizedKey() ?? "";
    }
}