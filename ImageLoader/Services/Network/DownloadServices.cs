using System.Collections.Concurrent;
using ImageLoader.BaseType;
using ImageLoader.Extension;
using ImageLoader.Services.Cache;

namespace ImageLoader.Services.Network;

public class DownloadServices : DisposableBase
{
    private string _path;
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _downloads = new();
    private readonly SemaphoreSlim _semaphore = new(5);
    private readonly MagicImageServices _magicImageServices;
    private CancellationTokenSource _cts = new();

    public DownloadServices(HttpClient client)
    {
        _path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ImageLoader",
            "Tmp");

        _magicImageServices = new MagicImageServices(_cts.Token);
        _client = client;
    }

    public async Task<string?> DownloadAsync(string key, int width)
    {
        var lazyTask = _downloads.GetOrAdd(key,
            k => new Lazy<Task<string?>>(() => GetBytesAsync(k, width), LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return await lazyTask.Value.ConfigureAwait(false);
        }
        finally
        {
            _downloads.TryRemove(key, out _);
        }
    }


    protected override void DisposeSpecific()
    {
        _cts.Cancel();
        _cts.Dispose();
        _semaphore.Dispose();
        _client.Dispose();
        _downloads.Clear();
    }

    private async Task<string?> GetBytesAsync(string fileName, int width)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using var response = await _client.SendAsync(
                    new HttpRequestMessage(HttpMethod.Get, fileName),
                    HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            Directory.CreateDirectory(_path);

            var path = Path.Combine(_path, fileName.Md5());
            
            await using var fileStream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await stream.CopyToAsync(fileStream).ConfigureAwait(false);

            await _magicImageServices.AddAsync(fileName, width);
            
            return path;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void TryDelete(FileInfo file)
    {
        try
        {
            file.Delete();
        }
        catch
        {
        }
    }
}