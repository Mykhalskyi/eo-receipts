namespace Receipts;

/// <summary>
/// Asks the secondary model when the primary one fails,
/// e.g. a local model first and a cloud one when it can't cope.
/// </summary>
public sealed class FallbackLlm : ILlm
{
    private readonly ILlm primary;
    private readonly ILlm secondary;

    public FallbackLlm(ILlm primary, ILlm secondary)
    {
        this.primary = primary;
        this.secondary = secondary;
    }

    public async Task<IJson> Answer(IPrompt prompt, CancellationToken ct = default)
    {
        try
        {
            var answer = await this.primary.Answer(prompt, ct);
            answer.Root();
            return answer;
        }
        catch (Exception e) when (e is ReceiptException || (e is OperationCanceledException && !ct.IsCancellationRequested))
        {
            return await this.secondary.Answer(prompt, ct);
        }
    }

    public IAcceptedTypes Accepts()
    {
        return new CommonTypes(this.primary.Accepts(), this.secondary.Accepts());
    }
}
