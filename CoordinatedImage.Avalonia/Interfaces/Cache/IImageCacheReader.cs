using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Interfaces.Cache;

public interface IImageCacheReader
{
    public Task<IRef<Bitmap>?> TryGetAsync(string key);
}

public interface IImageCacheWrite<T>
{
    public Task<bool> StoreAsync(string key, T reference);
}