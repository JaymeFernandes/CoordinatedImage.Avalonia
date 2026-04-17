using ImageLoader.Context.Model;
using ImageLoader.Extension;
using ImageLoader.Interfaces.Cache;
using Microsoft.EntityFrameworkCore;

namespace ImageLoader.Services.Cache;

public class CacheServices : ICacheServices
{
    public CacheServices()
    {
        ClearAllExpire();
    }
    
    public async Task<int> CountAsync()
    {
        await using var context = ImageDbFactory.Create();
        return await context.Files.CountAsync().ConfigureAwait(false);
    }

    public async Task<(bool found, FileModel? model)> TryGetImageAsync(string key, int width)
    {
        await using var context = ImageDbFactory.Create();

        var internalImage = new InternalImage();
        internalImage.Url = key;
        internalImage.Width = width;
        
        var image = await context.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == internalImage.Key)
            .ConfigureAwait(false);
        
        return (image != null, image);
    }

    public async Task<bool> TryRemoveImageAsync(FileModel image)
    {
        await using var context = ImageDbFactory.Create();
        
        context.Files.Remove(image);

        return await context.SaveChangesAsync().ConfigureAwait(false) > 0;
    }

    public async Task<bool> StoreImageAsync(string path, string url, bool persistent)
    {
        var info = new FileInfo(path);

        var image = new FileModel()
        {
            Path = info.FullName,
            Key = url.Md5(),
            Extension = info.Extension,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(persistent ? 365 : 1)
        };
        
        await using var context = ImageDbFactory.Create();
        
        context.Files.Add(image);
        
        return await context.SaveChangesAsync().ConfigureAwait(false) > 0;
    }

    private void ClearAllExpire()
    {
        using var context = ImageDbFactory.Create();
        var files = context.Files.Where(x => x.Expires <= DateTime.UtcNow);
        
        context.Files.RemoveRange(files);
        context.SaveChanges();
    }
}