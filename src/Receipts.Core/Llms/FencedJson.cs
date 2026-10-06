namespace Receipts;

/// <summary>
/// The JSON object found inside a text, from the first '{' to the last '}'.
/// </summary>
public sealed class FencedJson : IJson
{
    private readonly IJson origin;
    private readonly Lazy<IJson> clean;

    public FencedJson(IJson origin)
    {
        this.origin = origin;
        this.clean = new Lazy<IJson>(
            () =>
            {
                var text = origin.Text();
                var start = text.IndexOf('{');
                var end = text.LastIndexOf('}');
                return new JsonOf(start >= 0 && end > start ? text[start..(end + 1)] : text);
            }
        );
    }

    public string Text()
    {
        return this.clean.Value.Text();
    }

    public System.Text.Json.JsonElement Root()
    {
        return this.clean.Value.Root();
    }
}
