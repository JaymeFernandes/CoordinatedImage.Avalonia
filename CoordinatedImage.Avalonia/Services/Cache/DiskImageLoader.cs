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
    private readonly int _maxFiles;
    private readonly IMd5Helper _md5Helper;
    private readonly ConcurrentDictionary<string, Lazy<Task<IRef<Bitmap>?>>> _tasks = new();
    private readonly TimeSpan _timeToLive;

    private int _version;

    public DiskLoader(string diskCacheFolder = "Cache/Images", TimeSpan? timeToLive = null, int maxFiles = 300)
    {
        _diskCacheFolder = diskCacheFolder;
        Directory.CreateDirectory(_diskCacheFolder);
        _md5Helper = new Md5Helper();
        _timeToLive = timeToLive ?? TimeSpan.FromHours(1);
        _maxFiles = maxFiles;

        CleanExpiredOrExcessFiles();
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

    public Task<bool> StoreAsync(string key, string path)
    {
        var destiny = Path.Combine(_diskCacheFolder, _md5Helper.MD5(key));

        if (File.Exists(path))
            File.Move(path, destiny);

        return Task.FromResult(File.Exists(destiny));
    }

    private async Task<IRef<Bitmap>?> ReadFileAsync(string key)
    {
        var myVersion = Interlocked.Increment(ref _version);

        if (_version % 100 == 0)
            CleanExpiredOrExcessFiles();

        var path = Path.Combine(_diskCacheFolder, _md5Helper.MD5(key));
        var tmp = Path.Combine("Tmp", _md5Helper.MD5(key));

        var lastAccess = File.GetLastAccessTimeUtc(tmp);
        if (lastAccess < DateTime.UtcNow - _timeToLive)
            File.Delete(tmp);

        if (File.Exists(tmp))
        {
            File.SetLastAccessTimeUtc(tmp, DateTime.UtcNow);

            return RefCountable.Create(new Bitmap(tmp), key);
        }

        if (File.Exists(path))
        {
            File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
            
            using var fs = File.OpenRead(tmp);
            return RefCountable.Create(new Bitmap(fs), key);
        }

        return null;
    }

    private void CleanExpiredOrExcessFiles()
    {
        var files = new DirectoryInfo(_diskCacheFolder).GetFiles().OrderBy(f => f.LastAccessTimeUtc).ToList();
        var now = DateTime.UtcNow;
        foreach (var f in files)
            if (f.LastAccessTimeUtc < now - _timeToLive)
                try
                {
                    f.Delete();
                }
                catch
                {
                }

        files = new DirectoryInfo("Tmp").GetFiles().OrderBy(f => f.LastAccessTimeUtc).ToList();
        var excess = files.Count - _maxFiles;
        if (excess > 0)
            foreach (var f in files.Take(excess))
                try
                {
                    f.Delete();
                }
                catch
                {
                }
    }
}