using System.Collections.Immutable;
using System.Text.Json.Nodes;

namespace Receipts;

/// <summary>
/// Prompt parts collected into a JSON array, in the format of a particular model.
/// </summary>
public sealed class JsonPartMedia : IPartMedia<JsonArray>
{
    private readonly IPartFormat format;
    private readonly ImmutableList<JsonNode> nodes;

    public JsonPartMedia(IPartFormat format) : this(format, ImmutableList<JsonNode>.Empty)
    {
    }

    private JsonPartMedia(IPartFormat format, ImmutableList<JsonNode> nodes)
    {
        this.format = format;
        this.nodes = nodes;
    }

    public Task<IPartMedia<JsonArray>> Text(string text, CancellationToken ct = default)
    {
        return Task.FromResult<IPartMedia<JsonArray>>(new JsonPartMedia(this.format, this.nodes.Add(this.format.Text(text))));
    }

    public async Task<IPartMedia<JsonArray>> Image(IImage image, CancellationToken ct = default)
    {
        var bytes = await image.Bytes(ct);
        var type = await image.Type(ct);
        return new JsonPartMedia(this.format, this.nodes.Add(this.format.Image(type.Mime(), Convert.ToBase64String(bytes.Span))));
    }

    public JsonArray Value()
    {
        return new JsonArray(this.nodes.Select(node => node.DeepClone()).ToArray());
    }
}
