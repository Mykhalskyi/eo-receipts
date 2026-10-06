namespace Receipts;

/// <summary>
/// Receipts read by a language model.
/// </summary>
public sealed class LlmReceipts : IReceipts
{
    private readonly ILlm llm;
    private readonly Func<IImage, IPrompt> prompt;

    public LlmReceipts(ILlm llm) : this(llm, image => new ReceiptPrompt(image))
    {
    }

    public LlmReceipts(ILlm llm, Func<IImage, IPrompt> prompt)
    {
        this.llm = llm;
        this.prompt = prompt;
    }

    public async Task<IReceipt> Receipt(IImage image, CancellationToken ct = default)
    {
        var answer = await this.llm.Answer(this.prompt(image), ct);
        answer.Root();
        return new JsonReceipt(answer);
    }
}
