namespace Receipts;

/// <summary>
/// Receipts that passed <see cref="ValidReceipt"/> checks; invalid ones fail right away.
/// </summary>
public sealed class ValidReceipts : IReceipts
{
    private readonly IReceipts origin;
    private readonly decimal tolerance;

    public ValidReceipts(IReceipts origin) : this(origin, 0.02m)
    {
    }

    public ValidReceipts(IReceipts origin, decimal tolerance)
    {
        this.origin = origin;
        this.tolerance = tolerance;
    }

    public async Task<IReceipt> Receipt(IImage image, CancellationToken ct = default)
    {
        var receipt = new ValidReceipt(await this.origin.Receipt(image, ct), this.tolerance);
        receipt.Total();
        return receipt;
    }
}
