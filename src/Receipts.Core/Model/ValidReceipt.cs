namespace Receipts;

/// <summary>
/// Receipt that checks its own arithmetic before giving out any data:
/// each line is quantity × unit price, lines add up to the subtotal (or the total, when prices include taxes),
/// subtotal plus taxes (or the subtotal alone) equals the total, and the date is not in the future.
/// Throws <see cref="InvalidReceiptException"/> listing every violation.
/// </summary>
public sealed class ValidReceipt : IReceipt
{
    private readonly IReceipt origin;
    private readonly decimal tolerance;

    public ValidReceipt(IReceipt origin) : this(origin, 0.02m)
    {
    }

    public ValidReceipt(IReceipt origin, decimal tolerance)
    {
        this.origin = origin;
        this.tolerance = tolerance;
    }

    public IMerchant Merchant() => this.Checked().Merchant();

    public DateTimeOffset Date() => this.Checked().Date();

    public IReadOnlyList<ILine> Lines() => this.Checked().Lines();

    public IReadOnlyList<ITax> Taxes() => this.Checked().Taxes();

    public IMoney Subtotal() => this.Checked().Subtotal();

    public IMoney Total() => this.Checked().Total();

    public IPayment Payment() => this.Checked().Payment();

    public IMedia<T> Print<T>(IMedia<T> media) => this.Checked().Print(media);

    private IReceipt Checked()
    {
        var violations = new List<string>();
        var lines = this.origin.Lines();
        foreach (var line in lines)
        {
            var expected = line.Quantity() * line.UnitPrice().Amount();
            if (Math.Abs(expected - line.Total().Amount()) > this.tolerance)
            {
                violations.Add($"line '{line.Name()}': {line.Quantity()} × {line.UnitPrice().Amount()} is not {line.Total().Amount()}");
            }
        }
        var subtotal = this.origin.Subtotal().Amount();
        var total = this.origin.Total().Amount();
        var taxes = this.origin.Taxes().Sum(tax => tax.Amount().Amount());
        var sum = lines.Sum(line => line.Total().Amount());
        var slack = this.tolerance * Math.Max(1, lines.Count);
        if (lines.Count > 0 && Math.Abs(sum - subtotal) > slack && Math.Abs(sum - total) > slack)
        {
            violations.Add($"lines add up to {sum}, but subtotal is {subtotal} and total is {total}");
        }
        if (Math.Abs(subtotal + taxes - total) > slack && Math.Abs(subtotal - total) > slack)
        {
            violations.Add($"subtotal {subtotal} plus taxes {taxes} is not total {total}");
        }
        try
        {
            if (this.origin.Date() > DateTimeOffset.UtcNow.AddDays(1))
            {
                violations.Add($"date {this.origin.Date():yyyy-MM-dd} is in the future");
            }
        }
        catch (AbsentFieldException)
        {
            // a receipt without a date is not contradictory
        }
        if (violations.Count > 0)
        {
            throw new InvalidReceiptException(violations);
        }
        return this.origin;
    }
}
