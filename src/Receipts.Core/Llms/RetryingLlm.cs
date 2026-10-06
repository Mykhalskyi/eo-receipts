namespace Receipts;

/// <summary>
/// Repeats the call when it fails for a transient reason (network, timeout, 429, 5xx)
/// or when the answer is not valid JSON. The delay doubles after each attempt.
/// </summary>
public sealed class RetryingLlm : ILlm
{
    private readonly ILlm origin;
    private readonly int attempts;
    private readonly TimeSpan backoff;

    public RetryingLlm(ILlm origin) : this(origin, 3, TimeSpan.FromSeconds(1))
    {
    }

    public RetryingLlm(ILlm origin, int attempts, TimeSpan backoff)
    {
        this.origin = origin;
        this.attempts = attempts;
        this.backoff = backoff;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var answer = await this.origin.Answer(prompt, ct);
                answer.Root();
                return answer;
            }
            catch (Exception e) when (attempt < this.attempts && e is LlmException { Transient: true } or MalformedAnswerException)
            {
                await Task.Delay(this.backoff * Math.Pow(2, attempt - 1), ct);
            }
        }
    }

    public IAcceptedTypes Accepts()
    {
        return this.origin.Accepts();
    }
}
