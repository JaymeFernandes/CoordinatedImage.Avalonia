namespace CoordinatedImage.Avalonia.Interfaces.Utilities;

public interface IMd5Helper
{
    public string MD5(Stream stream);

    public string MD5(string stream);
}