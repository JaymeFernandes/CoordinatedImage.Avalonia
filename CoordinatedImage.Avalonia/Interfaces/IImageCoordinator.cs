using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Interfaces;

public interface IImageCoordinator
{
    public Task<IRef<Bitmap>?> LoadAsync(string? uri, IStorageProvider? storageprovider, bool saveToDisk = false);
    
    public Task<ICollection<string>> LoadChunkedAsync(string? uri, IStorageProvider? storageprovider, bool saveToDisk = false);
}