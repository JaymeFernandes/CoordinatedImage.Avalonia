using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CoordinatedImage.Avalonia.Interfaces;
using CoordinatedImage.Avalonia.Interfaces.Cache;
using CoordinatedImage.Avalonia.Services.Cache;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Coordinator.Shared;

public class ImageCoordinatorBase : IImageCoordinator
{
    protected readonly IDiskLoader DiskLoader;
    protected readonly ILocalImageLoader LocalImageLoader;

    public ImageCoordinatorBase(IDiskLoader diskLoader)
    {
        DiskLoader = diskLoader;
        LocalImageLoader = new LocalImageLoader();
    }
    
    public virtual async Task<IRef<Bitmap>?> LoadAsync(string? uri, IStorageProvider? storageprovider, bool saveToDisk = false)
    {
        var reference = await  LocalImageLoader.TryGetAsync(uri, storageprovider);
        
        if(reference != null)
            return reference;
        
        reference = await DiskLoader.TryGetAsync(uri);

        if (reference != null)
            return reference;

        var path = await ImageLoaderConfiguration.DownloadServices.DownloadAsync(uri);
        
        if (!string.IsNullOrWhiteSpace(path))
        {
            if(saveToDisk)
                await DiskLoader.StoreAsync(uri, path);
            
            reference = await DiskLoader.TryGetAsync(uri);

            if (!saveToDisk)
                await ImageLoaderConfiguration.DownloadServices.EnforceDiskLimitAsync();
            
            return reference;
        }
        
        return null;
    }

    public Task<ICollection<string>> LoadChunkedAsync(string? uri, IStorageProvider? storageprovider, bool saveToDisk = false)
    {
        throw new NotImplementedException();
    }
}