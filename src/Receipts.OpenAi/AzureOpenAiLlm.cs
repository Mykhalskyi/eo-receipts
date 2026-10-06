namespace Receipts.OpenAi;

/// <summary>
/// Model deployed in Azure OpenAI.
/// </summary>
public sealed class AzureOpenAiLlm : ILlm
{
    private readonly ILlm origin;

    public AzureOpenAiLlm(Uri endpoint, ICredential key, string deployment, HttpClient http)
        : this(endpoint, key, deployment, http, "2024-10-21")
    {
    }

    public AzureOpenAiLlm(Uri endpoint, ICredential key, string deployment, HttpClient http, string version)
    {
        this.origin = new ChatCompletions(
            http,
            new ChatAddress(endpoint, $"openai/deployments/{Uri.EscapeDataString(deployment)}/chat/completions?api-version={version}"),
            deployment,
            new ChatAuthorization("api-key", string.Empty, key),
            new AcceptedTypes("image/jpeg", "image/png", "image/webp", "image/gif"),
            "azure-openai"
        );
    }

    public Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default) => this.origin.Answer(prompt, ct);

    public IAcceptedTypes Accepts() => this.origin.Accepts();
}
