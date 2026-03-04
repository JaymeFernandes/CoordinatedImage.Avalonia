using System.Timers;
using Avalonia.Media.Imaging;
using BitFaster.Caching.Lru;
using CoordinatedImage.Avalonia.Interfaces.Cache;
using CoordinatedImage.Avalonia.Utilities;
using Timer = System.Timers.Timer;

namespace CoordinatedImage.Avalonia.Services.Cache;

public class MemoryLoader : IMemoryLoader
{
    private readonly ConcurrentTLru<string, IRef<Bitmap>> _cache;

    private readonly Timer _cleanupTimer;
    
    private readonly Timer _loadTimer;

    public MemoryLoader(int capacity, TimeSpan? expiration = null)
    {
        _cache = new ConcurrentTLru<string, IRef<Bitmap>>(capacity, expiration ?? TimeSpan.FromSeconds(30));

        _cache.Events.Value.ItemRemoved += (_, k) 
            =>
        {
            k.Value.Dispose();
        };

        _cleanupTimer = new Timer(20000);

        _cleanupTimer.Elapsed += Cleanup;
        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Enabled = true;
        
        _cleanupTimer.Start();
        
        _loadTimer = new Timer(1000);

        _loadTimer.Elapsed += Log;
        _loadTimer.AutoReset = true;
        _loadTimer.Enabled = true;
        
        _loadTimer.Start();
        
    }

    private void Log(Object source, ElapsedEventArgs e)
    {
        Console.Clear();

        foreach (var value in _cache)
        {
            Console.WriteLine($"{TruncateKey(value.Key)}: {value.Value.RefCount}");
        }
    }
    
    private void Cleanup(Object source, ElapsedEventArgs e)
    {
        var keys = _cache.Keys;

        foreach (var key in keys)
            _cache.TryGet(key, out _);
    }
    
    public Task<IRef<Bitmap>?> TryGetAsync(string key)
    {
        if (_cache.TryGet(key, out var reference))
            return Task.FromResult(reference.Clone());
        
        
        return Task.FromResult<IRef<Bitmap>>(null);
    }
    
    public Task<bool> StoreAsync(string key, IRef<Bitmap> reference)
    {
        _cache.AddOrUpdate(key, reference.Clone());
        return Task.FromResult(true);
    }

    public void Clear()
        => _cache.Clear();
    

    public int Count => _cache.Count;
    
    private static string TruncateKey(string key)
    {
        if (key.Length > 50)
            return key.Substring(0, 47) + "...";
        return key;
    }
}