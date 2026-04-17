using ImageLoader.Extension;
using ImageLoader.Interfaces.Cache;
using ImageLoader.Services.Cache;
using ImageLoader.Services.Network;

namespace ImageLoader.Services;

public class AsyncImageLoader
{
    public static AsyncImageLoader Instance { get; set; } = new AsyncImageLoader();
    
    private ICacheServices? _cacheServices;
    private readonly DownloadServices _downloadServices;

    public AsyncImageLoader()
    {
        _downloadServices = new DownloadServices(new HttpClient());
    }

    public AsyncImageLoader(ICacheServices cacheServices, DownloadServices downloadServices)
    {
        _downloadServices = downloadServices;
        InitializeCache(cacheServices);
    }

    public async Task<string?> LoadImageAsync(string url, int width, bool persistent)
    {
        if(_cacheServices == null)
            InitializeCache();
        
        if (File.Exists(url))
            return url;

        var (found, image) = await _cacheServices!
            .TryGetImageAsync(url, width)
            .ConfigureAwait(false);

        if (found && image != null)
        {
            if (image.Expires > DateTime.Now)
                return image.Path;
            else
                await _cacheServices.TryRemoveImageAsync(image)
                    .ConfigureAwait(false);
        }
        
        var path = await _downloadServices.DownloadAsync(url, width)
            .ConfigureAwait(false);

        if (path != null)
            await _cacheServices.StoreImageAsync(path, url, persistent);

        return path;
    }

    private void InitializeCache(ICacheServices? cacheServices = null)
    {
        var db = ImageDbFactory.Create();
        db.Database.EnsureCreated();

        _cacheServices = cacheServices ?? new CacheServices();
    }
    
}