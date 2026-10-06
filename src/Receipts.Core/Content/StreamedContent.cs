namespace Receipts;

/// <summary>
/// Content of a stream, read from its current position to the end.
/// A stream can be read only once, wrap it into <see cref="StickyContent"/>
/// when the bytes are needed more than once.
/// </summary>
public sealed class StreamedContent : IContent
{
    private readonly Stream stream;

    public StreamedContent(Stream stream)
    {
        this.stream = stream;
    }

    public async Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        using var memory = new MemoryStream();
        await this.stream.CopyToAsync(memory, ct);
        return memory.ToArray();
    }
}
