using ImageLoader.Context;
using Microsoft.EntityFrameworkCore;

namespace ImageLoader.Services.Cache;

public static class ImageDbFactory
{
    public static ImageDbContext Create()
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ImageLoader",
            "Tmp");

        Directory.CreateDirectory(basePath);

        return new ImageDbContext();
    }
}