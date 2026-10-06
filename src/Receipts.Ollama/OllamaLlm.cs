using System.Text.Json;
using System.Text.Json.Nodes;

namespace Receipts.Ollama;

/// <summary>
/// Vision model running locally in Ollama, e.g. qwen2.5vl, llama3.2-vision, gemma3, llava.
/// The answer is constrained by the JSON schema passed as <c>format</c>.
/// </summary>
public sealed class OllamaLlm : ILlm
{
    private readonly Uri host;
    private readonly string model;
    private readonly HttpClient http;

    public OllamaLlm(string model, HttpClient http) : this(new Uri("http://localhost:11434"), model, http)
    {
    }

    public OllamaLlm(Uri host, string model, HttpClient http)
    {
        this.host = host;
        this.model = model;
        this.http = http;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        var message = await new PrintedPrompt<JsonObject>(prompt, new OllamaMessage()).Value(ct);
        var body = new JsonObject
        {
            ["model"] = this.model,
            ["stream"] = false,
            ["format"] = prompt.Schema().Json(),
            ["options"] = new JsonObject { ["temperature"] = 0 },
            ["messages"] = new JsonArray(message),
        };
        var uri = new Uri($"{this.host.ToString().TrimEnd('/')}/api/chat");
        var reply = await new JsonEndpoint(this.http, uri, "ollama").Reply(body, ct);
        if (reply.TryGetProperty("done_reason", out var reason) && reason.GetString() == "length")
        {
            throw new MalformedAnswerException("ollama: the answer was cut by the token limit");
        }
        if (!reply.TryGetProperty("message", out var answer)
            || !answer.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.String)
        {
            throw new MalformedAnswerException("ollama: the answer has no message");
        }
        return new JsonOf(content.GetString()!);
    }

    public IAcceptedTypes Accepts()
    {
        return new AcceptedTypes("image/jpeg", "image/png");
    }
}
