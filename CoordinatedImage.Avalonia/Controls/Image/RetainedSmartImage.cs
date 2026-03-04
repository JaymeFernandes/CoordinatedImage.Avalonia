using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Controls.Image;

public class RetainedSmartImage : SmartImageBase<RetainedSmartImage>
{
    private CancellationTokenSource? _unloadDelayCts;

    protected override async Task LoadAsync(string? uri)
    {
        State = AsyncImageState.Loading;

        CancelImmediateUnload();
        CancelLoad();

        Coordinator ??= ImageLoaderConfiguration.Coordinator;

        var myVersion = Interlocked.Increment(ref Version);
        var cts = new CancellationTokenSource();
        Cts = cts;
        var token = cts.Token;

        if (string.IsNullOrWhiteSpace(uri))
        {
            ScheduleUnload();
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
                ScheduleUnload();
        }
    }

    private async void ScheduleUnload()
    {
        CancelImmediateUnload();

        _unloadDelayCts = new CancellationTokenSource();
        var token = _unloadDelayCts.Token;

        try
        {
            await Task.Delay(600, token);
        }
        catch
        {
            return;
        }

        if (token.IsCancellationRequested)
            return;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var oldRef = Ref;
            Ref = null;

            Image.Source = null;
            State = AsyncImageState.Idle;

            oldRef?.Dispose();
        });
    }

    private void CancelImmediateUnload()
    {
        _unloadDelayCts?.Cancel();
        _unloadDelayCts?.Dispose();
        _unloadDelayCts = null;
    }

    private void CancelLoad()
    {
        var old = Interlocked.Exchange(ref Cts, null);
        if (old == null) return;

        old.Cancel();
        old.Dispose();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelLoad();
        ScheduleUnload();
        base.OnDetachedFromVisualTree(e);
    }
}