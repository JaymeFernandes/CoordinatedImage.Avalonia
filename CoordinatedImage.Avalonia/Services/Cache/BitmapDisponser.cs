using System.Collections.Concurrent;
using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Utilities;

namespace CoordinatedImage.Avalonia.Services.Cache;

public static class BitmapDisposer
{
    private static readonly ConcurrentQueue<(IRef<Bitmap> bmp, long time)> Queue = new();
    private static readonly ConcurrentQueue<(RefCounter bmp, long time, Func<Task> func)> DisposeQueue = new();
    
    private static readonly ConcurrentDictionary<long, byte> Keys = new();
    
    private static int _running = 0;
    private static int _runningDispose = 0;

    public static void Schedule(IRef<Bitmap>? bitmap)
    {
        if (bitmap == null) return;

        Queue.Enqueue((bitmap, Environment.TickCount64));

        if (Interlocked.Exchange(ref _running, 1) == 0)
            _ = ProcessAsync();
    }
    
    public static void Schedule(this RefCounter counter, Func<Task> func)
    {
        if (!Keys.TryAdd(counter.Id, 0))
            return;

        DisposeQueue.Enqueue((counter, Environment.TickCount64, func));

        if (Interlocked.Exchange(ref _runningDispose, 1) == 0)
            _ = ProcessRefAsync();
    }

    private static async Task ProcessAsync()
    {
        while (Queue.TryDequeue(out var real))
        {
            try
            {
                real.bmp.Dispose();
            }
            catch { }
        }

        Interlocked.Exchange(ref _running, 0);
    }

    private static async Task ProcessRefAsync()
    {
        while (true)
        {
            await Task.Delay(200);

            var now = Environment.TickCount64;
            var processedAny = false;

            while (DisposeQueue.TryPeek(out var item))
            {
                if (now - item.time < TimeSpan.FromSeconds(10).TotalMilliseconds)
                    break;

                if (DisposeQueue.TryDequeue(out var real))
                {
                    processedAny = true;

                    try
                    {
                        if (real.bmp.CanDispose())
                            await real.func();
                    }
                    catch { }

                    Keys.TryRemove(real.bmp.Id, out _);
                }
            }

            if (!processedAny)
                break;
        }

        Interlocked.Exchange(ref _runningDispose, 0);
    }
}