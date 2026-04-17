using Avalonia.Media.Imaging;
using BitFaster.Caching;
using BitFaster.Caching.Lru;
using CoordinatedImage.Avalonia.Interfaces.Cache;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Services.Cache;

public class MemoryLoader : IMemoryLoader
{
    private readonly ConcurrentLru<string, IRef<Bitmap>> _cache;

    public MemoryLoader(int capacity)
    {
        _cache = new ConcurrentLru<string, IRef<Bitmap>>(capacity);

        _cache.Events.Value.ItemRemoved += OnRemoveEvent;
    }
    
    public int Count => _cache.Count;

    public Task<IRef<Bitmap>?> TryGetAsync(string key)
    {
        if (_cache.TryGet(key, out var reference))
            return Task.FromResult<IRef<Bitmap>?>(reference.Clone());

        return Task.FromResult<IRef<Bitmap>?>(null);
    }

    public Task<bool> StoreAsync(string key, IRef<Bitmap> reference)
    {
        if (_cache.TryGet(key, out _))
            return Task.FromResult(false);

        _cache.AddOrUpdate(key, reference.Clone());
        return Task.FromResult(true);
    }

    public void Dispose()
    {
        _cache.Clear();
    }

    public void Clear()
    {
        _cache.Clear();
    }

    private void OnRemoveEvent(object? sender, ItemRemovedEventArgs<string, IRef<Bitmap>> e)
    {
        if(e.Value != null)
            BitmapDisposer.Schedule(e.Value);
    }
}