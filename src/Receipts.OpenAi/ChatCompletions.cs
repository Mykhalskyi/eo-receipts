using System.Text.Json;
using System.Text.Json.Nodes;

namespace Receipts.OpenAi;

/// <summary>
/// A Chat Completions call with a strict JSON schema response format.
/// </summary>
public sealed class ChatCompletions : ILlm
{
    private readonly HttpClient http;
    private readonly ChatAddress address;
    private readonly string model;
    private readonly ChatAuthorization authorization;
    private readonly IAcceptedTypes types;
    private readonly string provider;

    public ChatCompletions(
        HttpClient http,
        ChatAddress address,
        string model,
        ChatAuthorization authorization,
        IAcceptedTypes types,
        string provider
    )
    {
        this.http = http;
        this.address = address;
        this.model = model;
        this.authorization = authorization;
        this.types = types;
        this.provider = provider;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        var content = await new PrintedPrompt<JsonArray>(prompt, new JsonPartMedia(new OpenAiFormat())).Value(ct);
        var body = new JsonObject
        {
            ["model"] = this.model,
            ["messages"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = content }),
            ["response_format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JsonObject
                {
                    ["name"] = "receipt",
                    ["strict"] = true,
                    ["schema"] = prompt.Schema().Json(),
                },
            },
        };
        var reply = await new JsonEndpoint(this.http, this.address.Uri(), this.provider, await this.authorization.Headers(ct))
            .Reply(body, ct);
        return new JsonOf(this.Text(reply));
    }

    public IAcceptedTypes Accepts()
    {
        return this.types;
    }

    private string Text(JsonElement reply)
    {
        if (!reply.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new MalformedAnswerException($"{this.provider}: the answer has no choices");
        }
        var choice = choices[0];
        var message = choice.GetProperty("message");
        if (message.TryGetProperty("refusal", out var refusal) && refusal.ValueKind == JsonValueKind.String)
        {
            throw new LlmException(this.provider, 200, $"The model refused to answer: {refusal.GetString()}");
        }
        if (choice.TryGetProperty("finish_reason", out var finish) && finish.GetString() == "length")
        {
            throw new MalformedAnswerException($"{this.provider}: the answer was cut by the token limit");
        }
        if (!message.TryGetProperty("content", out var text) || text.ValueKind != JsonValueKind.String)
        {
            throw new MalformedAnswerException($"{this.provider}: the answer has no text");
        }
        return text.GetString()!;
    }
}
