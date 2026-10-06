namespace Receipts;

/// <summary>
/// Cache in a directory, one file per answer; survives restarts.
/// </summary>
public sealed class FileCache : ICache
{
    private readonly string directory;

    public FileCache(string directory)
    {
        this.directory = directory;
    }

    public async Task<string> Value(string key, Func<CancellationToken, Task<string>> fresh, CancellationToken ct = default)
    {
        var path = Path.Combine(this.directory, $"{key}.json");
        if (File.Exists(path))
        {
            return await File.ReadAllTextAsync(path, ct);
        }
        var value = await fresh(ct);
        Directory.CreateDirectory(this.directory);
        var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(temporary, value, ct);
        File.Move(temporary, path, true);
        return value;
    }
}
