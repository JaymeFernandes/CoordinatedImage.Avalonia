using System.Collections.Concurrent;
using CoordinatedImage.Avalonia.Interfaces.Network;
using CoordinatedImage.Avalonia.Interfaces.Utilities;
using CoordinatedImage.Avalonia.Utilities.Helper;

namespace CoordinatedImage.Avalonia.Services.Network;

public class DownloadServices : IDownloadServices
{
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _downloads = new();
    private readonly SemaphoreSlim _semaphore = new(5);
    private readonly IMd5Helper _md5Helper;
    private readonly int _maxFiles;
    
    private const string _path = "Tmp";
    
    public DownloadServices(HttpClient client, int maxFiles = 40)
    {
        _maxFiles = maxFiles;
        _client = client;
        _md5Helper = new Md5Helper();
        
        EnforceDiskLimit();
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
            
            if(!Directory.Exists(_path))
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

    public Task EnforceDiskLimitAsync()
    {
        EnforceDiskLimit();
        
        return Task.CompletedTask;
    }

    public void EnforceDiskLimit()
    {
        var dir = new DirectoryInfo(_path);
        if (!dir.Exists)
            return;

        var files = dir.EnumerateFiles()
            .OrderBy(f => f.LastAccessTimeUtc)
            .ToList();

        foreach (var file in files.Where(x => DateTime.UtcNow > x.LastAccessTimeUtc + TimeSpan.FromMinutes(10)))
            TryDelete(file);

        if (files.Count <= _maxFiles)
            return;

        int filesToRemove = files.Count - _maxFiles;

        for (int i = 0; i < filesToRemove; i++)
        {
            TryDelete(files[i]);
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
    
    public void Dispose()
    {
        _client.Dispose();
        EnforceDiskLimit();
    }
}