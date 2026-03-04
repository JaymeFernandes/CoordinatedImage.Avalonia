namespace CoordinatedImage.Avalonia.Extensions;

public static class StringExtensions
{
    public static string ToSanitizedKey(this string key)
        => new string(key.ToCharArray()
            .Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            .ToArray());
}