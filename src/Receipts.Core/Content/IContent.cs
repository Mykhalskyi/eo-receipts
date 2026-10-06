namespace Receipts;

/// <summary>
/// Raw bytes coming from some source: a file, a stream, a URL, memory.
/// </summary>
public interface IContent
{
    Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default);
}
