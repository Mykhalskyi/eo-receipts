namespace Receipts;

/// <summary>
/// Content of a file on disk, read on demand.
/// </summary>
public sealed class FileContent : IContent
{
    private readonly string path;

    public FileContent(string path)
    {
        this.path = path;
    }

    public async Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return await File.ReadAllBytesAsync(this.path, ct);
    }
}
