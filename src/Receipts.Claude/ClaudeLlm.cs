using System.Text.Json.Nodes;

namespace Receipts.Claude;

/// <summary>
/// Claude model behind the Anthropic Messages API.
/// The answer is constrained by structured outputs (<c>output_config.format</c>).
/// </summary>
public sealed class ClaudeLlm : ILlm
{
    private readonly ICredential key;
    private readonly string model;
    private readonly HttpClient http;
    private readonly Uri endpoint;
    private readonly int tokens;

    public ClaudeLlm(ICredential key, HttpClient http) : this(key, "claude-opus-5-5", http)
    {
    }

    public ClaudeLlm(ICredential key, string model, HttpClient http)
        : this(key, model, http, new Uri("https://api.anthropic.com/v1/messages"), 16000)
    {
    }

    public ClaudeLlm(ICredential key, string model, HttpClient http, Uri endpoint, int tokens)
    {
        this.key = key;
        this.model = model;
        this.http = http;
        this.endpoint = endpoint;
        this.tokens = tokens;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        var content = await new PrintedPrompt<JsonArray>(prompt, new JsonPartMedia(new ClaudeFormat())).Value(ct);
        var body = new JsonObject
        {
            ["model"] = this.model,
            ["max_tokens"] = this.tokens,
            ["messages"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = content }),
            ["output_config"] = new JsonObject
            {
                ["format"] = new JsonObject { ["type"] = "json_schema", ["schema"] = prompt.Schema().Json() },
            },
        };
        var headers = new Dictionary<string, string>
        {
            ["x-api-key"] = await this.key.Value(ct),
            ["anthropic-version"] = "2023-06-01",
        };
        var reply = await new JsonEndpoint(this.http, this.endpoint, "claude", headers).Reply(body, ct);
        return new JsonOf(new ClaudeReply(reply).Text());
    }

    public IAcceptedTypes Accepts()
    {
        return new AcceptedTypes("image/jpeg", "image/png", "image/gif", "image/webp");
    }
}
