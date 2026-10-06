namespace Receipts.OpenAi;

/// <summary>
/// OpenAI model behind the Chat Completions API.
/// </summary>
public sealed class OpenAiLlm : ILlm
{
    private readonly ILlm origin;

    public OpenAiLlm(ICredential key, string model, HttpClient http) : this(key, model, http, new Uri("https://api.openai.com/v1/"))
    {
    }

    public OpenAiLlm(ICredential key, string model, HttpClient http, Uri root)
    {
        this.origin = new ChatCompletions(
            http,
            new ChatAddress(root, "chat/completions"),
            model,
            new ChatAuthorization("Authorization", "Bearer ", key),
            new AcceptedTypes("image/jpeg", "image/png", "image/webp", "image/gif"),
            "openai"
        );
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default) => this.origin.Answer(prompt, ct);

    public IAcceptedTypes Accepts() => this.origin.Accepts();
}
