namespace Receipts;

/// <summary>
/// Gives up when the model doesn't answer in time, raising a transient <see cref="LlmException"/>.
/// </summary>
public sealed class TimedLlm : ILlm
{
    private readonly ILlm origin;
    private readonly TimeSpan timeout;

    public TimedLlm(ILlm origin, TimeSpan timeout)
    {
        this.origin = origin;
        this.timeout = timeout;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        using var limited = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limited.CancelAfter(this.timeout);
        try
        {
            return await this.origin.Answer(prompt, limited.Token);
        }
        catch (OperationCanceledException e) when (!ct.IsCancellationRequested)
        {
            throw new LlmException("timeout", 408, $"No answer in {this.timeout}", e);
        }
    }

    public IAcceptedTypes Accepts()
    {
        return this.origin.Accepts();
    }
}
