using CoordinatedImage.Avalonia.Coordinator;
using CoordinatedImage.Avalonia.Interfaces;
using CoordinatedImage.Avalonia.Interfaces.Network;
using CoordinatedImage.Avalonia.Services.Cache;
using CoordinatedImage.Avalonia.Services.Network;

namespace CoordinatedImage.Avalonia;

public static class ImageLoaderConfiguration
{
    public static IImageCoordinator Coordinator { get; set; }
        = new SmartImageCoordinator(new DiskLoader(), new MemoryLoader(30));

    public static IDownloadServices DownloadServices { get; set; }
        = new DownloadServices(new HttpClient());
}