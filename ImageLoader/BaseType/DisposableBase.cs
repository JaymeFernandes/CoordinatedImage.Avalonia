namespace ImageLoader.BaseType;

public abstract class DisposableBase : IDisposable
{
    private int _disposed;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            DisposeSpecific();
            GC.SuppressFinalize(this);
        }
    }

    protected abstract void DisposeSpecific();

    protected void ThrowObjectDisposedExceptionIfNecessary()
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
    }
}