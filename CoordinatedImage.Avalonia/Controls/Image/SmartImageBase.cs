using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Interfaces;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Controls.Image;

public abstract class SmartImageBase<T> : Grid where T : Control
{
    protected int Version;
    protected IRef<Bitmap>? Ref;
    protected CancellationTokenSource? Cts;
    protected readonly ContentControl Loading;
    protected readonly global::Avalonia.Controls.Image Image;
    
    public static readonly StyledProperty<AsyncImageState> StateProperty =
        AvaloniaProperty.Register<T, AsyncImageState>(
            nameof(State),
            AsyncImageState.Idle);
    
    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<T, string?>(nameof(Source));
    
    public static readonly StyledProperty<IImageCoordinator?> CoordinatorProperty =
        AvaloniaProperty.Register<T, IImageCoordinator?>(nameof(Coordinator));

    public static readonly StyledProperty<IDataTemplate?> LoadingTemplateProperty 
        = AvaloniaProperty.Register<T, IDataTemplate?>(nameof(LoadingTemplate)); 
    
    public static readonly StyledProperty<IImage?> FallbackTemplateProperty 
        = AvaloniaProperty.Register<T, IImage?>(nameof(FallbackSource));

    public SmartImageBase()
    {
        Loading = new ContentControl() { IsVisible = true };
        Image = new global::Avalonia.Controls.Image { Stretch = Stretch.UniformToFill, IsVisible = false };
        
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

            var temp = TopLevel.GetTopLevel(this).StorageProvider;
            Loading.ContentTemplate = value;
            SetValue(LoadingTemplateProperty, value);
        }
    }

    public IImage? FallbackSource
    {
        get => GetValue(FallbackTemplateProperty);
        set =>SetValue(FallbackTemplateProperty, value);
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
            Image.Source = FallbackSource;
        
        Image.IsVisible = State == AsyncImageState.Success;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == SourceProperty)
            _ = LoadAsync(change.GetNewValue<string?>());
    }
}