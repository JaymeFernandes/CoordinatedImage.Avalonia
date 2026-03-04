namespace CoordinatedImage.Avalonia.Interfaces.Network;

public interface IDownloadServices : IDisposable
{
    Task<string?> DownloadAsync(string key);

    void EnforceDiskLimit();

    Task EnforceDiskLimitAsync();
}