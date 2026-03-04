using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Interfaces.Cache;

public interface IMemoryLoader : IImageCacheWrite<IRef<Bitmap>>, IImageCacheReader
{
    
}