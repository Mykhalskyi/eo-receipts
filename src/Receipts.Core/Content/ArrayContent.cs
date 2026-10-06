namespace Receipts;

/// <summary>
/// Content already in memory.
/// </summary>
public sealed class ArrayContent : IContent
{
    private readonly ReadOnlyMemory<byte> bytes;

    public ArrayContent(byte[] bytes) : this(new ReadOnlyMemory<byte>(bytes))
    {
    }

    public ArrayContent(ReadOnlyMemory<byte> bytes)
    {
        this.bytes = bytes;
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return Task.FromResult(this.bytes);
    }
}
