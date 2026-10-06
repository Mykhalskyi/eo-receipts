namespace Receipts;

public sealed class Money : IMoney
{
    private readonly decimal amount;
    private readonly ICurrency currency;

    public Money(decimal amount, string currency) : this(amount, new Currency(currency))
    {
    }

    public Money(decimal amount, ICurrency currency)
    {
        this.amount = amount;
        this.currency = currency;
    }

    public decimal Amount()
    {
        return this.amount;
    }

    public ICurrency Currency()
    {
        return this.currency;
    }

    public IMoney Plus(IMoney other)
    {
        if (!string.Equals(other.Currency().Code(), this.currency.Code(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Can't add {other.Currency().Code()} to {this.currency.Code()}");
        }
        return new Money(this.amount + other.Amount(), this.currency);
    }

    public override string ToString()
    {
        return $"{this.amount} {this.currency.Code()}";
    }
}
