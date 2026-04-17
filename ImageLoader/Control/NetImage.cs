using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ImageLoader.Services;
using ImageMagick;

namespace ImageLoader.Control;

public class NetImage : ContentControl
{
    public static readonly StyledProperty<IDataTemplate> ErrorTemplateProperty =
        AvaloniaProperty.Register<NetImage, IDataTemplate>(
            nameof(ErrorTemplate));

    public static readonly StyledProperty<IDataTemplate> LoadingTemplateProperty =
        AvaloniaProperty.Register<NetImage, IDataTemplate>(
            nameof(LoadingTemplate));

    public static readonly StyledProperty<bool> PersistedProperty =
        AvaloniaProperty.Register<NetImage, bool>
            (nameof(Persisted), false);


    public static readonly StyledProperty<string> SourceProperty = AvaloniaProperty.Register<NetImage, string>(
        nameof(Source));

    private readonly ContentControl _errorControl;
    private readonly Image _image;
    private readonly ContentControl _loadingControl;
    private Bitmap? _currentBitmap;
    private int _state;

    public NetImage()
    {
        _errorControl = new ContentControl();
        _loadingControl = new ContentControl();

        _image = new Image
        {
            Stretch = Stretch.Uniform
        };

        State = 0;

        var grid = new Grid();

        grid.Children.Add(CreateViewBox(_errorControl));
        grid.Children.Add(CreateViewBox(_loadingControl));
        grid.Children.Add(_image);

        Content = grid;
    }

    public IDataTemplate ErrorTemplate
    {
        get => GetValue(ErrorTemplateProperty);
        set => SetValue(ErrorTemplateProperty, value);
    }


    public IDataTemplate LoadingTemplate
    {
        get => GetValue(LoadingTemplateProperty);
        set
        {
            _loadingControl.ContentTemplate = value;
            SetValue(LoadingTemplateProperty, value);
        }
    }

    public int State
    {
        get => _state;
        private set
        {
            _state = value;

            _errorControl.IsVisible = value == -1;
            _loadingControl.IsVisible = value == 0;
            _image.IsVisible = value == 1;
        }
    }

    public bool Persisted
    {
        get => GetValue(PersistedProperty);
        set => SetValue(PersistedProperty, value);
    }

    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    protected override async void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SourceProperty)
        {
            var value = change.GetNewValue<string?>();
            
            if (string.IsNullOrWhiteSpace(value))
                ClearImage();
            else
            {
                var path = await AsyncImageLoader.Instance.LoadImageAsync(value, (int)Width, Persisted);
                
                if(string.IsNullOrWhiteSpace(path))
                    ClearImage();
                else
                    SetSource(path);
            }
                
        }

        base.OnPropertyChanged(change);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ClearImage();

        base.OnDetachedFromLogicalTree(e);
    }

    private void ClearImage()
    {
        _image.Source = null;

        _currentBitmap?.Dispose();
        _currentBitmap = null;
    }

    private void SetSource(string source)
    {
        State = 0;
        _currentBitmap?.Dispose();
        _currentBitmap = null;
        
        try
        {
            _currentBitmap = GetAndResize(source, (int)Width);
            _image.Source = _currentBitmap;
        }
        catch
        {
            State = -1;
            _image.Source = null;
            return;
        }

        State = 1;
    }

    private Viewbox CreateViewBox(ContentControl control)
    {
        var viewBox = new Viewbox
        {
            Stretch = Stretch.Uniform,
            Child = control
        };

        return viewBox;
    }

    private Bitmap? GetAndResize(string path, int width)
    {
        if (!File.Exists(path))
            return null;

        var info = new MagickImageInfo(path);
        try
        {
            if (width > 0 && info.Width > width)
            {
                using var stream = File.OpenRead(path);
                return Bitmap.DecodeToWidth(stream, width);
            }

            return new Bitmap(path);
        }
        catch
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
            }

            return null;
        }
    }
}