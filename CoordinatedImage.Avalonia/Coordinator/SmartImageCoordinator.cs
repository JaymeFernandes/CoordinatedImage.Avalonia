using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CoordinatedImage.Avalonia.Coordinator.Shared;
using CoordinatedImage.Avalonia.Interfaces.Cache;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Coordinator;

public class SmartImageCoordinator : ImageCoordinatorBase
{
    private readonly IMemoryLoader _memoryLoader;
    
    public SmartImageCoordinator(IDiskLoader diskLoader, IMemoryLoader memoryLoader) : base(diskLoader)
    {
        _memoryLoader = memoryLoader;
    }
    
    public override async Task<IRef<Bitmap>?> LoadAsync(string? uri, IStorageProvider? storageprovider, bool saveToDisk = false)
    {
        if(uri == null)
            return null;
        
        var reference = await _memoryLoader.TryGetAsync(uri);

        if (reference != null)
            return reference;
    

        reference = await base.LoadAsync(uri, storageprovider, saveToDisk);
        
        if(reference != null)
            await _memoryLoader.StoreAsync(uri, reference);
        
        return reference;
    }
}