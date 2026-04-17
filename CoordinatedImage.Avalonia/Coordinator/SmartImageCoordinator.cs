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

    public override async Task<IRef<Bitmap>?> LoadAsync(string? uri,
        IStorageProvider? storageprovider,
        CancellationToken ctx,
        bool saveToDisk = false)
    {
        ctx.ThrowIfCancellationRequested();

        if (uri == null)
            return null;

        ctx.ThrowIfCancellationRequested();

        var reference = await _memoryLoader.TryGetAsync(uri);

        if (reference != null)
            return reference;

        ctx.ThrowIfCancellationRequested();

        reference = await base.LoadAsync(uri, storageprovider, ctx, saveToDisk);

        if (reference != null)
        {
            ctx.ThrowIfCancellationRequested();
            await _memoryLoader.StoreAsync(uri, reference);
        }

        return reference;
    }
}