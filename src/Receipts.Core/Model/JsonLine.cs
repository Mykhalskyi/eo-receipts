using System.Text.Json;

namespace Receipts;

public sealed class JsonLine : ILine
{
    private readonly JsonElement element;
    private readonly ICurrency currency;

    public JsonLine(JsonElement element, ICurrency currency)
    {
        this.element = element;
        this.currency = currency;
    }

    public string Name() => new JsonField(this.element, "name").Text();

    public decimal Quantity() => new JsonField(this.element, "quantity").Number(1m);

    public IMoney UnitPrice()
    {
        var price = new JsonField(this.element, "unitPrice");
        if (price.Present())
        {
            return new Money(price.Number(), this.currency);
        }
        var quantity = this.Quantity();
        var total = this.Total().Amount();
        return new Money(quantity == 0 ? total : Math.Round(total / quantity, 4), this.currency);
    }

    public IMoney Total()
    {
        var total = new JsonField(this.element, "total");
        if (total.Present())
        {
            return new Money(total.Number(), this.currency);
        }
        return new Money(new JsonField(this.element, "unitPrice").Number() * this.Quantity(), this.currency);
    }

    public IMedia<T> Print<T>(IMedia<T> media)
    {
        return media
            .With("name", this.Name())
            .With("quantity", this.Quantity())
            .With("unitPrice", this.UnitPrice().Amount())
            .With("total", this.Total().Amount());
    }
}
