namespace Receipts;

/// <summary>
/// An image of a receipt.
/// </summary>
public interface IImage
{
    Task<ReadOnlyMemory<byte>> Bytes(CancellationToken ct = default);

    Task<IMediaType> Type(CancellationToken ct = default);
}
