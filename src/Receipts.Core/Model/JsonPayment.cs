using System.Text.Json;

namespace Receipts;

public sealed class JsonPayment : IPayment
{
    private readonly JsonElement element;

    public JsonPayment(JsonElement element)
    {
        this.element = element;
    }

    public string Method() => new JsonField(this.element, "method").Text();

    public string CardLast4() => new JsonField(this.element, "cardLast4").Text();

    public IMedia<T> Print<T>(IMedia<T> media)
    {
        return media.With("method", this.Method()).With("cardLast4", this.CardLast4());
    }
}
