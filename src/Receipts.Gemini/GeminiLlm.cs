using System.Text.Json.Nodes;

namespace Receipts.Gemini;

/// <summary>
/// Gemini model behind the Gemini API (generateContent),
/// answering according to <c>responseJsonSchema</c>.
/// </summary>
public sealed class GeminiLlm : ILlm
{
    private readonly ICredential key;
    private readonly string model;
    private readonly HttpClient http;
    private readonly Uri root;

    public GeminiLlm(ICredential key, string model, HttpClient http)
        : this(key, model, http, new Uri("https://generativelanguage.googleapis.com/v1beta/"))
    {
    }

    public GeminiLlm(ICredential key, string model, HttpClient http, Uri root)
    {
        this.key = key;
        this.model = model;
        this.http = http;
        this.root = root;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        var parts = await new PrintedPrompt<JsonArray>(prompt, new JsonPartMedia(new GeminiFormat())).Value(ct);
        var body = new JsonObject
        {
            ["contents"] = new JsonArray(new JsonObject { ["role"] = "user", ["parts"] = parts }),
            ["generationConfig"] = new JsonObject
            {
                ["responseMimeType"] = "application/json",
                ["responseJsonSchema"] = prompt.Schema().Json(),
            },
        };
        var uri = new Uri($"{this.root.ToString().TrimEnd('/')}/models/{Uri.EscapeDataString(this.model)}:generateContent");
        var headers = new Dictionary<string, string> { ["x-goog-api-key"] = await this.key.Value(ct) };
        var reply = await new JsonEndpoint(this.http, uri, "gemini", headers).Reply(body, ct);
        return new JsonOf(new GeminiReply(reply).Text());
    }

    public IAcceptedTypes Accepts()
    {
        return new AcceptedTypes("image/jpeg", "image/png", "image/webp", "image/heic", "image/heif");
    }
}
