using System.Text.Json;

namespace Receipts;

/// <summary>
/// JSON parsed lazily from text.
/// </summary>
public sealed class JsonOf : IJson
{
    private readonly string text;
    private readonly Lazy<JsonElement> root;

    public JsonOf(string text)
    {
        this.text = text;
        this.root = new Lazy<JsonElement>(
            () =>
            {
                try
                {
                    using var document = JsonDocument.Parse(text);
                    return document.RootElement.Clone();
                }
                catch (JsonException e)
                {
                    throw new MalformedAnswerException($"The answer is not JSON: {new Excerpt(text)}", e);
                }
            }
        );
    }

    public string Text()
    {
        return this.text;
    }

    public JsonElement Root()
    {
        return this.root.Value;
    }
}
