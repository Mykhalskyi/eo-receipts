namespace Receipts;

/// <summary>
/// Receipt that computes every value once and remembers it.
/// </summary>
public sealed class StickyReceipt : IReceipt
{
    private readonly IReceipt origin;
    private readonly Lazy<IMerchant> merchant;
    private readonly Lazy<DateTimeOffset> date;
    private readonly Lazy<IReadOnlyList<ILine>> lines;
    private readonly Lazy<IReadOnlyList<ITax>> taxes;
    private readonly Lazy<IMoney> subtotal;
    private readonly Lazy<IMoney> total;
    private readonly Lazy<IPayment> payment;

    public StickyReceipt(IReceipt origin)
    {
        this.origin = origin;
        this.merchant = new Lazy<IMerchant>(origin.Merchant);
        this.date = new Lazy<DateTimeOffset>(origin.Date);
        this.lines = new Lazy<IReadOnlyList<ILine>>(origin.Lines);
        this.taxes = new Lazy<IReadOnlyList<ITax>>(origin.Taxes);
        this.subtotal = new Lazy<IMoney>(origin.Subtotal);
        this.total = new Lazy<IMoney>(origin.Total);
        this.payment = new Lazy<IPayment>(origin.Payment);
    }

    public IMerchant Merchant() => this.merchant.Value;

    public DateTimeOffset Date() => this.date.Value;

    public IReadOnlyList<ILine> Lines() => this.lines.Value;

    public IReadOnlyList<ITax> Taxes() => this.taxes.Value;

    public IMoney Subtotal() => this.subtotal.Value;

    public IMoney Total() => this.total.Value;

    public IPayment Payment() => this.payment.Value;

    public IMedia<T> Print<T>(IMedia<T> media) => this.origin.Print(media);
}
