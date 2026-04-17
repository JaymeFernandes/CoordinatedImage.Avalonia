using System.Collections.Concurrent;
using CoordinatedImage.Avalonia.Interfaces.Network;
using CoordinatedImage.Avalonia.Interfaces.Utilities;
using CoordinatedImage.Avalonia.Utilities.Helper;

namespace CoordinatedImage.Avalonia.Services.Network;

public class DownloadServices : IDownloadServices
{
    private const string _path = "Tmp";
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _downloads = new();
    private readonly IMd5Helper _md5Helper;
    private readonly SemaphoreSlim _semaphore = new(5);

    public DownloadServices(HttpClient client)
    {
        _client = client;
        _md5Helper = new Md5Helper();
    }

    public async Task<string?> DownloadAsync(string key)
    {
        var lazyTask = _downloads.GetOrAdd(key,
            k => new Lazy<Task<string?>>(() => GetBytesAsync(k), LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return await lazyTask.Value;
        }
        catch
        {
            return null;
        }
        finally
        {
            _downloads.TryRemove(key, out _);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private async Task<string?> GetBytesAsync(string fileName)
    {
        await _semaphore.WaitAsync();

        try
        {
            using var response = await _client.SendAsync(
                    new HttpRequestMessage(HttpMethod.Get, fileName),
                    HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var path = Path.Combine(_path, _md5Helper.MD5(fileName));

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            using var fileStream = File.Create(path);
            await stream.CopyToAsync(fileStream);

            return path;
        }
        catch
        {
            return null;
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
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}