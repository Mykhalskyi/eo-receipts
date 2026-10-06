namespace Receipts;

/// <summary>
/// Payment that is not on the receipt.
/// </summary>
public sealed class NoPayment : IPayment
{
    public string Method()
    {
        return string.Empty;
    }

    public string CardLast4()
    {
        return string.Empty;
    }

    public IMedia<T> Print<T>(IMedia<T> media)
    {
        return media.With("method", string.Empty).With("cardLast4", string.Empty);
    }
}
