using ImageLoader.Context.Model;

namespace ImageLoader.Interfaces.Cache;

public interface ICacheServices
{
    public Task<int> CountAsync();
    
    public Task<(bool found, FileModel? model)> TryGetImageAsync(string key, int width);
    
    public Task<bool> TryRemoveImageAsync(FileModel image);
    
    public Task<bool> StoreImageAsync(string path, string url, bool persistent);
}