using System.Globalization;

namespace Receipts;

/// <summary>
/// Receipt read from the JSON answer of a model. Values are read on demand.
/// </summary>
public sealed class JsonReceipt : IReceipt
{
    private readonly IJson json;

    public JsonReceipt(IJson json)
    {
        this.json = json;
    }

    public IMerchant Merchant()
    {
        return new JsonMerchant(this.Field("merchant").Element());
    }

    public DateTimeOffset Date()
    {
        var text = this.Field("date").Text();
        if (text.Length == 0)
        {
            throw new AbsentFieldException("date");
        }
        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
        {
            throw new MalformedAnswerException($"'{text}' is not a date");
        }
        return date;
    }

    public IReadOnlyList<ILine> Lines()
    {
        var currency = this.Currency();
        return this.Field("lines").Items().Select(item => (ILine)new JsonLine(item, currency)).ToList();
    }

    public IReadOnlyList<ITax> Taxes()
    {
        var currency = this.Currency();
        return this.Field("taxes").Items().Select(item => (ITax)new JsonTax(item, currency)).ToList();
    }

    /// <summary>
    /// The printed subtotal; when absent, the sum of lines, or the total without taxes.
    /// </summary>
    public IMoney Subtotal()
    {
        var subtotal = this.Field("subtotal");
        if (subtotal.Present())
        {
            return new Money(subtotal.Number(), this.Currency());
        }
        var lines = this.Lines();
        if (lines.Count > 0)
        {
            return new Money(lines.Sum(line => line.Total().Amount()), this.Currency());
        }
        return new Money(this.Total().Amount() - this.Taxes().Sum(tax => tax.Amount().Amount()), this.Currency());
    }

    public IMoney Total()
    {
        return new Money(this.Field("total").Number(), this.Currency());
    }

    public IPayment Payment()
    {
        var payment = this.Field("payment");
        return payment.Present() ? new JsonPayment(payment.Element()) : new NoPayment();
    }

    public IMedia<T> Print<T>(IMedia<T> media)
    {
        return media
            .With("merchant", this.Merchant())
            .With("date", this.Field("date").Text())
            .With("currency", this.Currency().Code())
            .With("lines", this.Lines())
            .With("taxes", this.Taxes())
            .With("subtotal", this.Subtotal().Amount())
            .With("total", this.Total().Amount())
            .With("payment", this.Payment());
    }

    private ICurrency Currency()
    {
        var code = this.Field("currency").Text().Trim().ToUpperInvariant();
        return new Currency(code.Length == 0 ? "XXX" : code);
    }

    private JsonField Field(string name)
    {
        return new JsonField(this.json.Root(), name);
    }
}
