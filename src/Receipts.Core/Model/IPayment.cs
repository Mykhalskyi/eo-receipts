namespace Receipts;

/// <summary>
/// How the receipt was paid; every text is empty when unknown.
/// </summary>
public interface IPayment : IPrintable
{
    string Method();

    string CardLast4();
}
