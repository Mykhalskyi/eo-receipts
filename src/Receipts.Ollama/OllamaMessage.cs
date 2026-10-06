using System.Collections.Immutable;
using System.Text.Json.Nodes;

namespace Receipts.Ollama;

/// <summary>
/// User message of the Ollama chat API: texts joined into <c>content</c>,
/// images as Base64 strings in <c>images</c>.
/// </summary>
public sealed class OllamaMessage : IPartMedia<JsonObject>
{
    private readonly ImmutableList<string> texts;
    private readonly ImmutableList<string> images;

    public OllamaMessage() : this(ImmutableList<string>.Empty, ImmutableList<string>.Empty)
    {
    }

    private OllamaMessage(ImmutableList<string> texts, ImmutableList<string> images)
    {
        this.texts = texts;
        this.images = images;
    }

    public Task<IPartMedia<JsonObject>> Text(string text, CancellationToken ct = default)
    {
        return Task.FromResult<IPartMedia<JsonObject>>(new OllamaMessage(this.texts.Add(text), this.images));
    }

    public async Task<IPartMedia<JsonObject>> Image(IImage image, CancellationToken ct = default)
    {
        var bytes = await image.Bytes(ct);
        return new OllamaMessage(this.texts, this.images.Add(Convert.ToBase64String(bytes.Span)));
    }

    public JsonObject Value()
    {
        return new JsonObject
        {
            ["role"] = "user",
            ["content"] = string.Join("\n\n", this.texts),
            ["images"] = new JsonArray(this.images.Select(image => (JsonNode)JsonValue.Create(image)).ToArray()),
        };
    }
}
