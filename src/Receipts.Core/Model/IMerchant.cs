namespace Receipts;

/// <summary>
/// Seller; every text is empty when it is not on the receipt.
/// </summary>
public interface IMerchant : IPrintable
{
    string Name();

    string Address();

    string TaxId();
}
