namespace Receipts;

/// <summary>
/// Structured data of one receipt.
/// </summary>
public interface IReceipt : IPrintable
{
    IMerchant Merchant();

    /// <summary>
    /// Purchase date, throws <see cref="AbsentFieldException"/> when it is not on the receipt.
    /// </summary>
    DateTimeOffset Date();

    IReadOnlyList<ILine> Lines();

    IReadOnlyList<ITax> Taxes();

    IMoney Subtotal();

    IMoney Total();

    IPayment Payment();
}
