using System.Text.Json;

namespace Receipts;

public sealed class JsonTax : ITax
{
    private readonly JsonElement element;
    private readonly ICurrency currency;

    public JsonTax(JsonElement element, ICurrency currency)
    {
        this.element = element;
        this.currency = currency;
    }

    public string Name() => new JsonField(this.element, "name").Text();

    public decimal Rate() => new JsonField(this.element, "rate").Number(0m);

    public IMoney Amount() => new Money(new JsonField(this.element, "amount").Number(), this.currency);

    public IMedia<T> Print<T>(IMedia<T> media)
    {
        return media.With("name", this.Name()).With("rate", this.Rate()).With("amount", this.Amount().Amount());
    }
}
