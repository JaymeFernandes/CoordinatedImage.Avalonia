using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Interfaces;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Controls;

public abstract class SmartImageBase<T> : Grid where T : Control
{
    public static readonly StyledProperty<IImageCoordinator?> CoordinatorProperty =
        AvaloniaProperty.Register<T, IImageCoordinator?>(nameof(Coordinator));

    public static readonly StyledProperty<IImage?> FallbackTemplateProperty
        = AvaloniaProperty.Register<T, IImage?>(nameof(FallbackSource));

    public static readonly StyledProperty<IDataTemplate?> LoadingTemplateProperty
        = AvaloniaProperty.Register<T, IDataTemplate?>(nameof(LoadingTemplate));

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<T, string?>(nameof(Source));

    public static readonly StyledProperty<AsyncImageState> StateProperty =
        AvaloniaProperty.Register<T, AsyncImageState>(
            nameof(State));

    protected readonly Image Image;
    
    protected readonly ContentControl Loading;
    
    protected CancellationTokenSource? Cts;
    
    protected IRef<Bitmap>? Ref;
    
    protected int Version;

    public SmartImageBase()
    {
        Loading = new ContentControl { IsVisible = true };
        Image = new Image
        {
            Stretch = Stretch.UniformToFill, 
            IsVisible = false,
        };
        
        RenderOptions.SetBitmapInterpolationMode(Image, BitmapInterpolationMode.LowQuality);
        

        Children.Add(Loading);
        Children.Add(Image);
    }

    public IImageCoordinator? Coordinator
    {
        get => GetValue(CoordinatorProperty);
        set => SetValue(CoordinatorProperty, value);
    }

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public IDataTemplate? LoadingTemplate
    {
        get => GetValue(LoadingTemplateProperty);
        set
        {
            Loading.ContentTemplate = value;
            SetValue(LoadingTemplateProperty, value);
        }
    }

    public IImage? FallbackSource
    {
        get => GetValue(FallbackTemplateProperty);
        set => SetValue(FallbackTemplateProperty, value);
    }

    public AsyncImageState State
    {
        get => GetValue(StateProperty);
        protected set
        {
            SetValue(StateProperty, value);
            UpdateVisualState();
        }
    }

    protected abstract Task LoadAsync(string? url);

    protected void UpdateVisualState()
    {
        Loading.IsVisible = State == AsyncImageState.Loading;

        if (State == AsyncImageState.Error)
        {
            Image.Source = null; 
            Image.Source = FallbackSource;
        }

        Image.IsVisible = State == AsyncImageState.Success;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SourceProperty)
        {
            var newValue = change.GetNewValue<string?>();
            var oldValue = change.GetOldValue<string?>();

            if (newValue != oldValue)
            {
                Image.Source = null;
                _ = LoadAsync(newValue);
            }
        }
        
        base.OnPropertyChanged(change);
    }
}