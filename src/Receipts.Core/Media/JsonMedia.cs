using System.Collections.Immutable;
using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// Prints into a JSON object.
/// </summary>
public sealed class JsonMedia : IMedia<JsonObject>
{
    private readonly ImmutableList<KeyValuePair<string, JsonNode>> properties;

    public JsonMedia() : this(ImmutableList<KeyValuePair<string, JsonNode>>.Empty)
    {
    }

    private JsonMedia(ImmutableList<KeyValuePair<string, JsonNode>> properties)
    {
        this.properties = properties;
    }

    public IMedia<JsonObject> With(string name, string value) => this.With(name, JsonValue.Create(value));

    public IMedia<JsonObject> With(string name, decimal value) => this.With(name, JsonValue.Create(value));

    public IMedia<JsonObject> With(string name, IPrintable section) => this.With(name, section.Print(new JsonMedia()).Value());

    public IMedia<JsonObject> With(string name, IEnumerable<IPrintable> items)
    {
        return this.With(name, new JsonArray(items.Select(item => (JsonNode)item.Print(new JsonMedia()).Value()).ToArray()));
    }

    public JsonObject Value()
    {
        var json = new JsonObject();
        foreach (var property in this.properties)
        {
            json[property.Key] = property.Value.DeepClone();
        }
        return json;
    }

    private JsonMedia With(string name, JsonNode node)
    {
        return new JsonMedia(this.properties.Add(new KeyValuePair<string, JsonNode>(name, node)));
    }
}
