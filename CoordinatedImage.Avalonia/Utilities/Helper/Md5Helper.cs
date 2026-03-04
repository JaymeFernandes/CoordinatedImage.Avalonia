using System.Text;
using CoordinatedImage.Avalonia.Interfaces.Utilities;
using CoordinatedImage.Avalonia.Extensions;

namespace CoordinatedImage.Avalonia.Utilities.Helper;

public class Md5Helper : IMd5Helper
{
    public string MD5(Stream stream)
    {
        using var hashProvider = System.Security.Cryptography.MD5.Create();
        
        var bytes = hashProvider.ComputeHash(stream);
        return BitConverter.ToString(bytes)?.ToSanitizedKey() ?? "";
    }

    public string MD5(string input)
    {
        using var hashProvider = System.Security.Cryptography.MD5.Create();
        
        var bytes = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(bytes)?.ToSanitizedKey() ?? "";
    }
}