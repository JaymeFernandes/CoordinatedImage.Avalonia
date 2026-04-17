using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CoordinatedImage.Avalonia.Services.Cache;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Controls;

public class SmartImage : SmartImageBase<SmartImage>
{
    private async Task UpdateUiAsync(IRef<Bitmap>? newRef, AsyncImageState state, long? versionCheck = null)
    {
        var current = Ref;

        if (newRef != null && current != null && newRef.Key == current.Key)
        {
            BitmapDisposer.Schedule(newRef);
            return;
        }

        if (versionCheck.HasValue && versionCheck.Value != Version)
        {
            if (newRef != null)
                BitmapDisposer.Schedule(newRef);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (versionCheck.HasValue && versionCheck.Value != Version)
            {
                if (newRef != null)
                    BitmapDisposer.Schedule(newRef);
                return;
            }

            var oldRef = Interlocked.Exchange(ref Ref, newRef);

            Image.Source = newRef?.Item;
            State = state;

            if (oldRef != null)
                BitmapDisposer.Schedule(oldRef);
        });
    }

    private void Cancel()
    {
        var old = Interlocked.Exchange(ref Cts, null);

        if (old == null) return;

        old.Cancel();
        old.Dispose();
    }

    protected override async Task LoadAsync(string? uri)
    {
        State = AsyncImageState.Loading;

        Cancel();

        var coordinator = Coordinator ?? ImageLoaderConfiguration.Coordinator;

        var myVersion = Interlocked.Increment(ref Version);

        var cts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref Cts, cts);

        oldCts?.Cancel();
        oldCts?.Dispose();

        var token = cts.Token;

        if (string.IsNullOrWhiteSpace(uri))
        {
            await UpdateUiAsync(null, AsyncImageState.Idle);
            return;
        }

        IRef<Bitmap>? newRef = null;

        try
        {
            newRef = await coordinator.LoadAsync(
                uri,
                TopLevel.GetTopLevel(this)?.StorageProvider,
                token
            );

            if (newRef == null)
            {
                await UpdateUiAsync(null, AsyncImageState.Error);
                return;
            }

            if (token.IsCancellationRequested || myVersion != Version)
            {
                BitmapDisposer.Schedule(newRef);
                return;
            }

            await UpdateUiAsync(newRef, AsyncImageState.Success, myVersion);
        }
        catch
        {
            if (!token.IsCancellationRequested)
                await UpdateUiAsync(null, AsyncImageState.Error);
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        Cancel();

        var oldRef = Interlocked.Exchange(ref Ref, null);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Image.Source = null;

            if (oldRef != null)
                BitmapDisposer.Schedule(oldRef);
        });

        base.OnDetachedFromLogicalTree(e);
    }
}