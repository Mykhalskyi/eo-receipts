namespace Receipts;

/// <summary>
/// Content that is read from its origin once and remembered afterwards.
/// </summary>
public sealed class StickyContent : IContent
{
    private readonly Lazy<Task<ReadOnlyMemory<byte>>> bytes;

    public StickyContent(IContent origin)
    {
        this.bytes = new Lazy<Task<ReadOnlyMemory<byte>>>(() => origin.Bytes(CancellationToken.None));
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return this.bytes.Value.WaitAsync(ct);
    }
}
