using System.Runtime.ConstrainedExecution;
using Avalonia.Media.Imaging;

namespace CoordinatedImage.Avalonia.Utilities;

public interface IRef<out T> : IDisposable where T : class
{
    T Item { get; }
        
    IRef<T> Clone();

    IRef<TResult> CloneAs<TResult>() where TResult : class;
        
    int RefCount { get; }
}

internal static class RefCountable
{
    public static IRef<Bitmap> Create(Bitmap item, string key)
    {
        return new Ref<Bitmap>(item, new RefCounter(item, key));
    }
}

public class RefCounter : IDisposable
{
    private IDisposable? _item;
    private volatile int _refs;
    private readonly long _id;
    private string? _key;

    public RefCounter(IDisposable item, string? key = null)
    {
        
        
        _item = item;
        _refs = 1;
        _key = key;
        
        _id = Interlocked.Increment(ref _nextId);
    }

    public long Id => _id;
    public string? Key => _key;

    public void SetKey(string key)
    {
        _key = key;
    }

    public void AddRef()
    {
        var old = _refs;
        while (true)
        {
            if (old == 0)
            {
                throw new ObjectDisposedException("Cannot add a reference to a nonreferenced item");
            }
            var current = Interlocked.CompareExchange(ref _refs, old + 1, old);
            if (current == old)
            {
                break;
            }
            old = current;
        }
    }

    public void Release()
    {
        var old = _refs;
        while (true)
        {
            var current = Interlocked.CompareExchange(ref _refs, old - 1, old);

            if (current == old)
            {
                if (old == 1)
                {
                    _item?.Dispose();
                    
                    _item = null;
                }
                break;
            }
            old = current;
        }
    }

    internal int RefCount => _refs;

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    private static long _nextId;
}

public class Ref<T> : CriticalFinalizerObject, IRef<T> where T : class
{
    protected T? item;
    private readonly RefCounter _counter;
    private readonly object _lock = new object();

    public Ref(T item, RefCounter counter)
    {
        this.item = item;
        _counter = counter;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (item != null)
            {
                _counter.Release();
                item = null;
            }
            GC.SuppressFinalize(this);
        }
    }

    ~Ref()
    {
        Dispose();
    }

    public T Item
    {
        get
        {
            lock (_lock)
            {
                return item!;
            }
        }
    }
    
    public IRef<T> Clone()
    {
        lock (_lock)
        {
            if (item != null)
            {
                var newRef = new Ref<T>(item, _counter);
                _counter.AddRef();
                return newRef;
            }
            throw new ObjectDisposedException("Ref<" + typeof(T) + ">");
        }
    }

    public IRef<TResult> CloneAs<TResult>() where TResult : class
    {
        lock (_lock)
        {
            if (item != null)
            {
                var castRef = new Ref<TResult>((TResult)(object)item, _counter);
                Interlocked.MemoryBarrier();
                _counter.AddRef();
                return castRef;
            }
            throw new ObjectDisposedException("Ref<" + typeof(T) + ">");
        }
    }

    public int RefCount => _counter.RefCount;
}

