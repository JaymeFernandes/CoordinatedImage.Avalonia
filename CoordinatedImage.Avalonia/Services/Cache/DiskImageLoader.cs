using System.Collections.Concurrent;
using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Interfaces.Cache;
using CoordinatedImage.Avalonia.Interfaces.Utilities;
using CoordinatedImage.Avalonia.Utilities;
using CoordinatedImage.Avalonia.Utilities.Helper;

namespace CoordinatedImage.Avalonia.Services.Cache;

public class DiskLoader : IDiskLoader
{
    private readonly string _diskCacheFolder;
    private readonly IMd5Helper _md5Helper;
    private readonly ConcurrentDictionary<string, Lazy<Task<IRef<Bitmap>?>>> _tasks = new();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);
    
    public DiskLoader(
        string diskCacheFolder = "Cache/Images")
    {
        _diskCacheFolder = diskCacheFolder;
        _md5Helper = new Md5Helper();
    }

    public async Task<IRef<Bitmap>?> TryGetAsync(string key)
    {
        var lazyTask = _tasks.GetOrAdd(key,
            k => 
                new Lazy<Task<IRef<Bitmap>?>>(() => ReadFileAsync(k), LazyThreadSafetyMode.ExecutionAndPublication));
        
        try
        {
            return await lazyTask.Value;
        }
        catch
        {
            return null;
        }
        finally
        {
            _tasks.TryRemove(key, out _);
        }
    }

    private async Task<IRef<Bitmap>?> ReadFileAsync(string key)
    {
        await _semaphore.WaitAsync();

        try
        {
            var path = Path.Combine(_diskCacheFolder, _md5Helper.MD5(key));
            var tmp = Path.Combine("Tmp",  _md5Helper.MD5(key));

            if (File.Exists(tmp))
            {
                File.SetLastAccessTimeUtc(tmp, DateTime.UtcNow);
                
                return RefCountable.Create(new Bitmap(tmp), key);
            }
                
            if (File.Exists(path))
            {
                File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
            
                return RefCountable.Create(new Bitmap(path), key);
            }

            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task<bool> StoreAsync(string key, string path)
    {
        var destiny = Path.Combine(_diskCacheFolder, _md5Helper.MD5(key));
        
        if(File.Exists(path))
            File.Move(path, destiny);

        return Task.FromResult(File.Exists(destiny));
    }
}