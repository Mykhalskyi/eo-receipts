using System.Text.Json;

namespace Receipts;

public sealed class JsonMerchant : IMerchant
{
    private readonly JsonElement element;

    public JsonMerchant(JsonElement element)
    {
        this.element = element;
    }

    public string Name() => new JsonField(this.element, "name").Text();

    public string Address() => new JsonField(this.element, "address").Text();

    public string TaxId() => new JsonField(this.element, "taxId").Text();

    public IMedia<T> Print<T>(IMedia<T> media)
    {
        return media.With("name", this.Name()).With("address", this.Address()).With("taxId", this.TaxId());
    }
}
