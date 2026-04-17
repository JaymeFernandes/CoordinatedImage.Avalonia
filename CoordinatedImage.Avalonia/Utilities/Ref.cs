using System.Runtime.ConstrainedExecution;
using Avalonia.Media.Imaging;
using CoordinatedImage.Avalonia.Services.Cache;

namespace CoordinatedImage.Avalonia.Utilities;

public interface IRef<out T> : IDisposable where T : class
{
    T Item { get; }
    
    string Key { get; }

    int RefCount { get; }

    IRef<T> Clone();
    
    public bool IsDisponse { get; }

    IRef<TResult> CloneAs<TResult>() where TResult : class;
}

internal static class RefCountable
{
    public static IRef<Bitmap> Create(Bitmap item, string key)
    {
        return new Ref<Bitmap>(key, item, new RefCounter(item, key));
    }
}

public class RefCounter : IDisposable
{
    private static long _nextId;
    private IDisposable? _item;
    private volatile int _refs;
    public bool IsDisponse => _disposeScheduled == 1;
    
    private int _disposeScheduled = 0;

    public RefCounter(IDisposable item, string? key = null)
    {
        _item = item;
        _refs = 1;
        Key = key;

        Id = Interlocked.Increment(ref _nextId);
    }

    public long Id { get; }

    public string? Key { get; private set; }

    internal int RefCount => _refs;

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    public void SetKey(string key)
    {
        Key = key;
    }

    public void AddRef()
    {
        var old = _refs;
        while (true)
        {
            if (old == 0) throw new ObjectDisposedException("Cannot add a reference to a nonreferenced item");
            var current = Interlocked.CompareExchange(ref _refs, old + 1, old);
            if (current == old) break;
            old = current;
        }
    }

    public void Release()
    {
        var old = _refs;
        while (true)
        {
            if (old == 0)
                return;
            
            var current = Interlocked.CompareExchange(ref _refs, old - 1, old);

            if (current == old)
            {
                if (old == 1)
                {
                    if (Interlocked.Exchange(ref _disposeScheduled, 1) == 0)
                    {
                        this.Schedule(async () =>
                        {
                            _item?.Dispose();
                            _item = null;
                            await Task.CompletedTask;
                        });
                    }
                }

                return;
            }

            old = current;
        }
    }
    
    public bool CanDispose()
    {
        return Volatile.Read(ref _refs) == 0;
    }
}

public class Ref<T> : IRef<T> where T : class
{
    private readonly RefCounter _counter;
    private readonly object _lock = new();
    protected T? InternalItem;

    public string Key { get; private set; }

    public bool IsDisponse => _counter.IsDisponse;

    public Ref(string key, T item, RefCounter counter)
    {
        Key = key;
        InternalItem = item;
        _counter = counter;
    }

    public T Item => InternalItem!;

    public int RefCount => _counter.RefCount;

    public void Dispose()
    {
        lock (_lock)
        {
            if (InternalItem != null)
            {
                _counter.Release();
                InternalItem = null;
            }

            GC.SuppressFinalize(this);
        }
    }

    public IRef<T> Clone()
    {
        lock (_lock)
        {
            if (InternalItem != null)
            {
                var newRef = new Ref<T>(Key, InternalItem, _counter);
                _counter.AddRef();
                return newRef;
            }

            throw new ObjectDisposedException("Ref<" + typeof(T) + ">");
        }
    }

    public IRef<TResult> CloneAs<TResult>() where TResult : class
    {
        if (InternalItem is not TResult cast)
            throw new InvalidCastException();

        _counter.AddRef();
        return new Ref<TResult>(Key, cast, _counter);
    }
}