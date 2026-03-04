using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Controls.Image;

public class SmartImage : SmartImageBase<SmartImage>
{
    protected override async Task LoadAsync(string? uri)
    {
        State = AsyncImageState.Loading;

        Cancel();

        Coordinator ??= ImageLoaderConfiguration.Coordinator;

        var myVersion = Interlocked.Increment(ref Version);
        var cts = new CancellationTokenSource();
        Cts = cts;
        var token = cts.Token;

        if (string.IsNullOrWhiteSpace(uri))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Image.Source = null;
                State = AsyncImageState.Idle;
            });

            return;
        }

        try
        {
            var newRef = await Coordinator.LoadAsync(uri, TopLevel.GetTopLevel(this)?.StorageProvider);

            if (newRef == null)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Image.Source = null;
                    State = AsyncImageState.Error;
                });

                return;
            }

            if (token.IsCancellationRequested || myVersion != Version)
            {
                newRef.Dispose();
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (myVersion != Version)
                {
                    newRef.Dispose();
                    return;
                }

                var oldRef = Ref;

                Ref = newRef;
                Image.Source = Ref.Item;
                State = AsyncImageState.Success;

                oldRef?.Dispose();
            });
        }
        catch
        {
            if (!token.IsCancellationRequested)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Image.Source = null;
                    State = AsyncImageState.Error;
                });
            }
        }
    }

    private void Cancel()
    {
        var old = Interlocked.Exchange(ref Cts, null);
        
        if (old == null) return;
        old.Cancel();
        old.Dispose();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Cancel();
        
        var oldRef = Ref;
        Ref = null;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Image.Source = null;
            oldRef?.Dispose();
        });
        
        base.OnDetachedFromVisualTree(e);
    }
}