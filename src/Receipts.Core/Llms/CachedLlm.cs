namespace Receipts;

/// <summary>
/// Remembers answers by a hash of the prompt (schema, texts and image bytes),
/// so the same receipt is never paid for twice.
/// </summary>
public sealed class CachedLlm : ILlm
{
    private readonly ILlm origin;
    private readonly ICache cache;
    private readonly string scope;

    public CachedLlm(ILlm origin, ICache cache) : this(origin, cache, string.Empty)
    {
    }

    /// <param name="origin">The model to ask on a cache miss.</param>
    /// <param name="cache">Where answers are kept.</param>
    /// <param name="scope">Distinguishes answers of different models sharing one cache.</param>
    public CachedLlm(ILlm origin, ICache cache, string scope)
    {
        this.origin = origin;
        this.cache = cache;
        this.scope = scope;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        var key = await new PromptHash(prompt, this.scope).Value(ct);
        var text = await this.cache.Value(
            key,
            async token =>
            {
                var answer = await this.origin.Answer(prompt, token);
                answer.Root();
                return answer.Text();
            },
            ct
        );
        return new JsonOf(text);
    }

    public IAcceptedTypes Accepts()
    {
        return this.origin.Accepts();
    }
}
