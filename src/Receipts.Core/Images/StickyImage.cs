namespace Receipts;

/// <summary>
/// Image whose bytes and type are evaluated once and remembered.
/// Useful in front of expensive conversions.
/// </summary>
public sealed class StickyImage : IImage
{
    private readonly Lazy<Task<ReadOnlyMemory<byte>>> bytes;
    private readonly Lazy<Task<IMediaType>> type;

    public StickyImage(IImage origin)
    {
        this.bytes = new Lazy<Task<ReadOnlyMemory<byte>>>(() => origin.Bytes(CancellationToken.None));
        this.type = new Lazy<Task<IMediaType>>(() => origin.Type(CancellationToken.None));
    }

    public Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default)
    {
        return this.bytes.Value.WaitAsync(ct);
    }

    public Task<IMediaType> Type(CancellationToken ct = default)
    {
        return this.type.Value.WaitAsync(ct);
    }
}
