namespace Receipts;

/// <summary>
/// Limits the number of concurrent calls, to stay within rate limits
/// or to protect a local model from overload.
/// </summary>
public sealed class ThrottledLlm : ILlm
{
    private readonly ILlm origin;
    private readonly SemaphoreSlim slots;

    public ThrottledLlm(ILlm origin, int concurrency) : this(origin, new SemaphoreSlim(concurrency, concurrency))
    {
    }

    public ThrottledLlm(ILlm origin, SemaphoreSlim slots)
    {
        this.origin = origin;
        this.slots = slots;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        await this.slots.WaitAsync(ct);
        try
        {
            return await this.origin.Answer(prompt, ct);
        }
        finally
        {
            this.slots.Release();
        }
    }

    public IAcceptedTypes Accepts()
    {
        return this.origin.Accepts();
    }
}
