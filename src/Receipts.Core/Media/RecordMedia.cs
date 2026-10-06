using System.Text.Json;
using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// Prints into a plain C# record, for serializers, ORMs and web frameworks.
/// </summary>
public sealed class RecordMedia : IMedia<ReceiptRecord>
{
    private readonly IMedia<JsonObject> json;

    public RecordMedia() : this(new JsonMedia())
    {
    }

    private RecordMedia(IMedia<JsonObject> json)
    {
        this.json = json;
    }

    public IMedia<ReceiptRecord> With(string name, string value) => new RecordMedia(this.json.With(name, value));

    public IMedia<ReceiptRecord> With(string name, decimal value) => new RecordMedia(this.json.With(name, value));

    public IMedia<ReceiptRecord> With(string name, IPrintable section) => new RecordMedia(this.json.With(name, section));

    public IMedia<ReceiptRecord> With(string name, IEnumerable<IPrintable> items) => new RecordMedia(this.json.With(name, items));

    public ReceiptRecord Value()
    {
        return this.json.Value().Deserialize<ReceiptRecord>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new MalformedAnswerException("The receipt can't be represented as a record");
    }
}
