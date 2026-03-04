using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Interfaces.Cache;

public interface ILocalImageLoader
{
    public Task<IRef<Bitmap>?> TryGetAsync(string? key, IStorageProvider? serviceProvider);
}