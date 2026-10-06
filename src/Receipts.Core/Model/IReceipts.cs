namespace Receipts;

/// <summary>
/// All receipts that can be read from images.
/// </summary>
public interface IReceipts
{
    Task<IReceipt> Receipt(IImage image, CancellationToken ct = default);
}
