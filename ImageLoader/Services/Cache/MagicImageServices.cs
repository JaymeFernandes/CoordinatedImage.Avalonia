using System.Collections.Concurrent;
using ImageLoader.Context.Model;
using ImageLoader.Extension;
using ImageMagick;

namespace ImageLoader.Services.Cache;

public class MagicImageServices
{
    private ConcurrentQueue<InternalImage> _queue = new();
    
    private ConcurrentDictionary<string, Task> _inProgress = new();
    
    private SemaphoreSlim _semaphore = new(3);
    
    private string _cachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ImageLoader",
        "Tmp");

    public MagicImageServices(CancellationToken token)
    {
        _ = RunService(token);
    }

    public Task AddAsync(string url, int width)
    {
        _queue.Enqueue(new InternalImage
        {
            Url = url,
            Width = width,
        });
        
        return Task.CompletedTask;
    }

    private async Task RunService(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var image))
            {
                try
                {
                    await ProcessImage(image);
                }
                finally
                {
                    _inProgress.TryRemove(image.Key, out _);
                }
            }
            else
            {
                await Task.Delay(10, token);
            }
        }
    }
    
    private async Task ProcessImage(InternalImage image)
    {
        var targetSize = image.Width;

        var cachePath = Path.Combine(_cachePath, image.Key);

        if (File.Exists(cachePath))
            return;

        var sourcePath = await image.GetBestSource(_cachePath);

        using var magick = new MagickImage(sourcePath);

        magick.Resize((uint)targetSize, 0); 
        magick.Strip();
        magick.Quality = 85;

        await magick.WriteAsync(cachePath);

        using var db = ImageDbFactory.Create();
        
        var model = new FileModel()
        {
            Key = image.Key,
            Path = Path.Combine(_cachePath, image.Key),
            Extension = "",
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(3)
        };

        await db.AddAsync(model);
        await db.SaveChangesAsync();
    }
    
    
}

internal class InternalImage
{
    public string Url { get; set; }

    public string OriginalKey => Url.Md5();
    public string Key => $"{Url.Md5()}_{Width}";

    private int _width;

    public int Width
    {
        get => _width;
        set => _width = Normalize(value);
    }

    private static readonly int[] Sizes = [64, 256, 512, 1024];

    private static int Normalize(int width)
    {
        foreach (var size in Sizes)
        {
            if (width <= size)
                return size;
        }

        return Sizes[^1];
    }
    
    public async Task<string> GetBestSource(string path)
    {
        foreach (var size in Sizes.Reverse())
        {
            if (size >= Width)
            {
                var file = System.IO.Path.Combine(path, $"{Url.Md5()}_{size}");
                
                if (File.Exists(file))
                    return file;
            }
        }
        
        return Path.Combine(path, Url.Md5());
    }
}