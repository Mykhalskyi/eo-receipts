namespace Receipts.OpenAi;

/// <summary>
/// Local or self-hosted server with an OpenAI-compatible API:
/// LM Studio (http://localhost:1234/v1), vLLM, llama.cpp server, LocalAI.
/// The server must support <c>response_format</c> with a JSON schema;
/// wrap into <see cref="FencedJsonLlm"/> otherwise.
/// </summary>
public sealed class OpenAiCompatibleLlm : ILlm
{
    private readonly ILlm origin;

    public OpenAiCompatibleLlm(Uri root, string model, HttpClient http) : this(root, model, http, new NoCredential())
    {
    }

    public OpenAiCompatibleLlm(Uri root, string model, HttpClient http, ICredential key)
        : this(root, model, http, key, new AcceptedTypes("image/jpeg", "image/png"))
    {
    }

    public OpenAiCompatibleLlm(Uri root, string model, HttpClient http, ICredential key, IAcceptedTypes types)
    {
        this.origin = new ChatCompletions(
            http,
            new ChatAddress(root, "chat/completions"),
            model,
            new ChatAuthorization("Authorization", "Bearer ", key),
            types,
            "openai-compatible"
        );
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default) => this.origin.Answer(prompt, ct);

    public IAcceptedTypes Accepts() => this.origin.Accepts();
}
