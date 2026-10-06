using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// JSON Schema written as text.
/// </summary>
public sealed class JsonSchema : IJsonSchema
{
    private readonly string text;

    public JsonSchema(string text)
    {
        this.text = text;
    }

    public JsonObject Json()
    {
        return JsonNode.Parse(this.text)!.AsObject();
    }
}
