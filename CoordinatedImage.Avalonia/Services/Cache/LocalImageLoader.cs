using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CoordinatedImage.Avalonia.Interfaces.Cache;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Services.Cache;

public class LocalImageLoader : ILocalImageLoader
{
    public async Task<IRef<Bitmap>?> TryGetAsync(string? key, IStorageProvider? provider)
    {
        if(string.IsNullOrWhiteSpace(key))
            return null;
        
        if (File.Exists(key))
            return RefCountable.Create(new Bitmap(key), key);
        
        if (provider is null) 
            return null;

        try
        {
            var fileInfo = await provider.TryGetFileFromPathAsync(key);
            
            if(fileInfo is null)
                return null;
            using var fileStream = await fileInfo.OpenReadAsync();

            return RefCountable.Create(new Bitmap(fileStream), key);
        }
        catch
        {
            return null;
        }
        
    }
}