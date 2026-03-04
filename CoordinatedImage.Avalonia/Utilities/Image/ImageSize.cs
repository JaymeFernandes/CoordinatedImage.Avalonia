using Ardalis.SmartEnum;

namespace CoordinatedImage.Avalonia.Utilities.Image;

public class ImageTransform : SmartEnum<ImageTransform>
{
    public int Width { get; }

    public ImageSize GetImageSize(int originalWidth, int originalHeight, int quality = 100)
        => new ImageSize(Width, (originalHeight * Width) / originalWidth, quality);
    
    public ImageTransform(string name, int value, int width) : base(name, value)
    {
        Width = width;
    }
    
    public static readonly ImageTransform Mini = new("mini", 0, 150);
    public static readonly ImageTransform Small = new("Small", 1, 300);
    public static readonly ImageTransform Medium = new("Medium", 2, 600);
    public static readonly ImageTransform Large = new("Large", 3, 1200);
}

public record ImageSize(int Width, int Resize, int Quality);