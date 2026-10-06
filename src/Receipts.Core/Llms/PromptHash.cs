namespace Receipts;

/// <summary>
/// SHA-256 of everything a prompt sends to a model: scope, schema, texts and images.
/// </summary>
public sealed class PromptHash
{
    private readonly IPrompt prompt;
    private readonly string scope;

    public PromptHash(IPrompt prompt) : this(prompt, string.Empty)
    {
    }

    public PromptHash(IPrompt prompt, string scope)
    {
        this.prompt = prompt;
        this.scope = scope;
    }

    public async Task<string> Value(CancellationToken ct = default)
    {
        var media = await new HashMedia().Text(this.scope, ct);
        media = await media.Text(this.prompt.Schema().Json().ToJsonString(), ct);
        return await new PrintedPrompt<string>(this.prompt, media).Value(ct);
    }
}
