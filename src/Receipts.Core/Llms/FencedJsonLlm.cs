namespace Receipts;

/// <summary>
/// Cleans answers of models without structured output support:
/// drops Markdown fences and any prose around the JSON object.
/// </summary>
public sealed class FencedJsonLlm : ILlm
{
    private readonly ILlm origin;

    public FencedJsonLlm(ILlm origin)
    {
        this.origin = origin;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        return new FencedJson(await this.origin.Answer(prompt, ct));
    }

    public IAcceptedTypes Accepts()
    {
        return this.origin.Accepts();
    }
}
